using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenCand.Config;
using OpenCand.ETL.Database;
using OpenCand.ETL.Parsing;
using OpenCand.Parser.Models;

namespace OpenCand.ETL.Loading
{
    /// <summary>
    /// Loads consulta_cand files: identity resolution in memory (<see cref="CandidateResolver"/>), then per batch
    /// COPY into temp tables + one set-based upsert per table, in a single transaction.
    /// </summary>
    public sealed class CandidatoLoader
    {
        private readonly NpgsqlDataSource dataSource;
        private readonly EtlSettings settings;
        private readonly ILogger<CandidatoLoader> logger;

        public CandidatoLoader(NpgsqlDataSource dataSource, IOptions<EtlSettings> settings, ILogger<CandidatoLoader> logger)
        {
            this.dataSource = dataSource;
            this.settings = settings.Value;
            this.logger = logger;
        }

        public async Task<LoadStats> LoadAsync(string directory, CancellationToken ct)
        {
            var stats = new LoadStats("candidatos");
            if (!Directory.Exists(directory))
            {
                logger.LogWarning("Candidatos: directory not found at {Directory}, skipping", directory);
                return stats;
            }

            var files = CsvSource.ListFiles(directory, logger);
            stats.Files = files.Count;
            if (files.Count == 0)
            {
                logger.LogWarning("Candidatos: no CSV files in {Directory}", directory);
                return stats;
            }

            var resolver = new CandidateResolver();
            await PreloadAsync(resolver, ct);
            logger.LogInformation("Candidatos: preloaded {Count} existing candidatos", resolver.CandidatoCount);

            var encoding = Encoding.GetEncoding(settings.CsvEncoding);
            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                var timer = Stopwatch.StartNew();
                var readBefore = stats.Read;
                logger.LogInformation("Candidatos: loading {File}", file);

                foreach (var row in CsvSource.Read<CandidatoCSV>(file, encoding, stats, logger))
                {
                    if (!TryMap(row, out var record, out var reason))
                    {
                        stats.Reject(reason);
                        continue;
                    }

                    resolver.Add(record);
                    stats.AddWritten();

                    if (resolver.PendingRows >= settings.BatchSize)
                        await FlushAsync(resolver.TakeBatch(), ct);
                }

                await FlushAsync(resolver.TakeBatch(), ct);
                LogFile("Candidatos", file, stats.Read - readBefore, timer);
            }

            return stats;
        }

        private async Task PreloadAsync(CandidateResolver resolver, CancellationToken ct)
        {
            var candidatos = new List<ExistingCandidate>();
            await using (var command = dataSource.CreateCommand(
                "SELECT idcandidato, cpf, nome, datanascimento, sexo, apelido, localidade, ultimoano FROM candidato"))
            await using (var reader = await command.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    candidatos.Add(new ExistingCandidate(
                        reader.GetGuid(0),
                        reader.IsDBNull(1) ? null : reader.GetString(1),
                        reader.GetString(2),
                        reader.IsDBNull(3) ? null : reader.GetFieldValue<DateOnly>(3),
                        reader.IsDBNull(4) ? null : reader.GetString(4),
                        reader.IsDBNull(5) ? null : reader.GetString(5),
                        reader.IsDBNull(6) ? null : reader.GetString(6),
                        reader.IsDBNull(7) ? 0 : reader.GetInt32(7)));
                }
            }

            var mappings = new List<ExistingMapping>();
            await using (var command = dataSource.CreateCommand(
                "SELECT idcandidato, nome, ano, cargo, siglauf, nomeue, nrcandidato, resultado FROM candidato_mapping"))
            await using (var reader = await command.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    mappings.Add(new ExistingMapping(reader.GetGuid(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3),
                        reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7)));
                }
            }

            resolver.Preload(candidatos, mappings);
        }

        internal static bool TryMap(CandidatoCSV c, out CandidateRecord record, out string reason)
        {
            record = default!;

            var nome = TseParsing.NullIfTseEmpty(c.NomeCandidato);
            if (nome is null) { reason = "invalid_name"; return false; }

            var sq = TseParsing.NullIfBlank(c.SequencialCandidato);
            var turno = TseParsing.NullIfBlank(c.Turno);
            var siglaUf = TseParsing.NullIfBlank(c.SiglaUF);
            var nomeUe = TseParsing.NullIfBlank(c.NomeUE);
            var cargo = TseParsing.NullIfBlank(c.DescricaoCargo);
            var nr = TseParsing.NullIfBlank(c.NumeroCandidato);
            var siglaPartido = TseParsing.NullIfBlank(c.SiglaPartido);
            var nomePartido = TseParsing.NullIfBlank(c.NomePartido);
            if (sq is null || turno is null || siglaUf is null || nomeUe is null || cargo is null || nr is null ||
                siglaPartido is null || nomePartido is null)
            {
                reason = "missing_field";
                return false;
            }

            reason = string.Empty;
            record = new CandidateRecord(
                Ano: c.AnoEleicao,
                Sq: sq,
                Turno: turno,
                TipoEleicao: TseParsing.NullIfBlank(c.TipoAbrangencia),
                SiglaUf: siglaUf,
                NomeUe: nomeUe,
                Cargo: cargo,
                NrCandidato: nr,
                Nome: nome,
                Apelido: TseParsing.NullIfTseEmpty(c.Apelido),
                Cpf: TseParsing.ParseCpf(c.CPFCandidato),
                Email: TseParsing.NullIfTseEmpty(c.Email),
                Dob: TseParsing.ParseDate(c.DataNascimento),
                Sexo: TseParsing.NullIfBlank(c.Genero),
                Ocupacao: TseParsing.NullIfBlank(c.Ocupacao),
                EstadoCivil: TseParsing.NullIfBlank(c.EstadoCivil),
                Escolaridade: TseParsing.NullIfBlank(c.GrauInstrucao),
                Resultado: TseParsing.NullIfTseEmpty(c.SituacaoTurno) ?? "-",
                SiglaPartido: siglaPartido,
                NomePartido: nomePartido,
                NumeroPartido: c.NumeroPartido);
            return true;
        }

        private async Task FlushAsync(CandidateBatch batch, CancellationToken ct)
        {
            if (batch.IsEmpty) return;

            await using var connection = await dataSource.OpenConnectionAsync(ct);
            await using var tx = await connection.BeginTransactionAsync(ct);

            await connection.ExecuteAsync("CREATE TEMP TABLE stg_partido (LIKE partido INCLUDING DEFAULTS) ON COMMIT DROP", ct);
            await connection.ExecuteAsync("CREATE TEMP TABLE stg_candidato (LIKE candidato INCLUDING DEFAULTS) ON COMMIT DROP", ct);
            await connection.ExecuteAsync("CREATE TEMP TABLE stg_mapping (LIKE candidato_mapping INCLUDING DEFAULTS) ON COMMIT DROP", ct);
            await connection.ExecuteAsync("CREATE TEMP TABLE stg_ext (LIKE candidato_ext INCLUDING DEFAULTS) ON COMMIT DROP", ct);

            if (batch.Partidos.Count > 0)
            {
                await using var w = await connection.BeginBinaryImportAsync("COPY stg_partido (sigla, nome, numero) FROM STDIN (FORMAT BINARY)", ct);
                foreach (var p in batch.Partidos)
                {
                    w.StartRow();
                    w.WriteText(p.Sigla, 50);
                    w.WriteText(p.Nome, 255);
                    w.WriteInt(p.Numero);
                }
                await w.CompleteAsync(ct);
            }

            if (batch.Candidatos.Count > 0)
            {
                await using var w = await connection.BeginBinaryImportAsync(
                    "COPY stg_candidato (idcandidato, cpf, nome, datanascimento, sexo, apelido, localidade, ultimoano) FROM STDIN (FORMAT BINARY)", ct);
                foreach (var c in batch.Candidatos)
                {
                    w.StartRow();
                    w.WriteGuid(c.Id);
                    w.WriteText(c.Cpf, 11);
                    w.WriteText(c.Nome, 255);
                    w.WriteDate(c.Dob);
                    w.WriteText(c.Sexo, 20);
                    w.WriteText(c.Apelido, 255);
                    w.WriteText(c.Localidade, 100);
                    w.WriteInt(c.UltimoAno);
                }
                await w.CompleteAsync(ct);
            }

            if (batch.Mappings.Count > 0)
            {
                await using var w = await connection.BeginBinaryImportAsync(
                    "COPY stg_mapping (idcandidato, cpf, nome, sqcandidato, turno, ano, tipoeleicao, siglauf, nomeue, cargo, sgpartido, nrcandidato, resultado) FROM STDIN (FORMAT BINARY)", ct);
                foreach (var m in batch.Mappings)
                {
                    w.StartRow();
                    w.WriteGuid(m.Id);
                    w.WriteText(m.Cpf, 11);
                    w.WriteText(m.Nome, 255);
                    w.WriteText(m.Sq, 50);
                    w.WriteText(m.Turno, 2);
                    w.WriteInt(m.Ano);
                    w.WriteText(m.TipoEleicao, 50);
                    w.WriteText(m.SiglaUf, 2);
                    w.WriteText(m.NomeUe, 100);
                    w.WriteText(m.Cargo, 50);
                    w.WriteText(m.SiglaPartido, 50);
                    w.WriteText(m.NrCandidato, 20);
                    w.WriteText(m.Resultado, 50);
                }
                await w.CompleteAsync(ct);
            }

            if (batch.Exts.Count > 0)
            {
                await using var w = await connection.BeginBinaryImportAsync(
                    "COPY stg_ext (idcandidato, ano, apelido, email, estadocivil, escolaridade, ocupacao) FROM STDIN (FORMAT BINARY)", ct);
                foreach (var e in batch.Exts)
                {
                    w.StartRow();
                    w.WriteGuid(e.Id);
                    w.WriteInt(e.Ano);
                    w.WriteText(e.Apelido, 255);
                    w.WriteText(e.Email);
                    w.WriteText(e.EstadoCivil, 50);
                    w.WriteText(e.Escolaridade, 50);
                    w.WriteText(e.Ocupacao);
                }
                await w.CompleteAsync(ct);
            }

            // IS DISTINCT FROM keeps unchanged rows untouched, so re-running the ETL does not bloat the tables.
            await connection.ExecuteAsync(@"
                INSERT INTO partido (sigla, nome, numero)
                SELECT sigla, nome, numero FROM stg_partido
                ON CONFLICT (sigla) DO UPDATE SET nome = EXCLUDED.nome, numero = EXCLUDED.numero
                WHERE (partido.nome, partido.numero) IS DISTINCT FROM (EXCLUDED.nome, EXCLUDED.numero);", ct);

            await connection.ExecuteAsync(@"
                INSERT INTO candidato (idcandidato, cpf, nome, datanascimento, sexo, apelido, localidade, ultimoano)
                SELECT idcandidato, cpf, nome, datanascimento, sexo, apelido, localidade, ultimoano FROM stg_candidato
                ON CONFLICT (idcandidato) DO UPDATE SET
                    cpf = EXCLUDED.cpf, nome = EXCLUDED.nome, datanascimento = EXCLUDED.datanascimento, sexo = EXCLUDED.sexo,
                    apelido = EXCLUDED.apelido, localidade = EXCLUDED.localidade, ultimoano = EXCLUDED.ultimoano
                WHERE (candidato.cpf, candidato.nome, candidato.datanascimento, candidato.sexo, candidato.apelido, candidato.localidade, candidato.ultimoano)
                      IS DISTINCT FROM
                      (EXCLUDED.cpf, EXCLUDED.nome, EXCLUDED.datanascimento, EXCLUDED.sexo, EXCLUDED.apelido, EXCLUDED.localidade, EXCLUDED.ultimoano);", ct);

            await connection.ExecuteAsync(@"
                INSERT INTO candidato_mapping (idcandidato, cpf, nome, sqcandidato, turno, ano, tipoeleicao, siglauf, nomeue, cargo, sgpartido, nrcandidato, resultado)
                SELECT idcandidato, cpf, nome, sqcandidato, turno, ano, tipoeleicao, siglauf, nomeue, cargo, sgpartido, nrcandidato, resultado FROM stg_mapping
                ON CONFLICT DO NOTHING;", ct);

            await connection.ExecuteAsync(@"
                INSERT INTO candidato_ext (idcandidato, ano, apelido, email, estadocivil, escolaridade, ocupacao)
                SELECT idcandidato, ano, apelido, email, estadocivil, escolaridade, ocupacao FROM stg_ext
                ON CONFLICT (idcandidato, ano) DO UPDATE SET
                    apelido = EXCLUDED.apelido, email = EXCLUDED.email, estadocivil = EXCLUDED.estadocivil,
                    escolaridade = EXCLUDED.escolaridade, ocupacao = EXCLUDED.ocupacao
                WHERE (candidato_ext.apelido, candidato_ext.email, candidato_ext.estadocivil, candidato_ext.escolaridade, candidato_ext.ocupacao)
                      IS DISTINCT FROM
                      (EXCLUDED.apelido, EXCLUDED.email, EXCLUDED.estadocivil, EXCLUDED.escolaridade, EXCLUDED.ocupacao);", ct);

            await tx.CommitAsync(ct);
        }

        private void LogFile(string dataset, string file, long rows, Stopwatch timer)
        {
            var seconds = Math.Max(timer.Elapsed.TotalSeconds, 0.001);
            logger.LogInformation("{Dataset}: {File} done, {Rows} rows in {Seconds:F1}s ({Rate:N0} rows/s)",
                dataset, Path.GetFileName(file), rows, seconds, rows / seconds);
        }
    }
}
