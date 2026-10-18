using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenCand.Config;
using OpenCand.ETL.Database;
using OpenCand.ETL.Parsing;
using OpenCand.Parser.Models;

namespace OpenCand.ETL.Loading
{
    public sealed record ReceitaRow(Guid IdCandidato, int Ano, string Turno, string Sq, string SgPartido, string? Fonte, string? Origem,
        string? Natureza, string? Especie, string? Cnpj, string? Cpf, string? Nome, string? NomeRfb, string? Municipio, DateTime? Data,
        string? Descricao, decimal? Valor);

    public sealed class ReceitaLoader : StreamingFactLoader<ReceitasCSV, ReceitaRow>
    {
        public ReceitaLoader(NpgsqlDataSource dataSource, SqIndex sqIndex, IOptions<EtlSettings> settings, ILogger<ReceitaLoader> logger)
            : base(dataSource, sqIndex, settings, logger) { }

        public override string Dataset => "receitas";
        public override SwapTable Table => SwapTable.Receitas;

        protected override string CopyCommand =>
            "COPY receitas_candidato_new (idcandidato, ano, turno, sqcandidato, sgpartido, fontereceita, origemreceita, naturezareceita, " +
            "especiereceita, cnpjdoador, cpfdoador, nomedoador, nomedoadorrfb, municipiodoador, datareceita, descricao, valor) " +
            "FROM STDIN (FORMAT BINARY)";

        protected override int Ano(ReceitasCSV csv) => csv.AnoEleicao;
        protected override string? Sq(ReceitasCSV csv) => TseParsing.NullIfBlank(csv.SequencialCandidato);

        protected override bool TryPrepare(ReceitasCSV csv, Guid idCandidato, out ReceitaRow row, out string reason)
        {
            row = default!;
            reason = string.Empty;

            var sgPartido = TseParsing.NullIfBlank(csv.SiglaPartido);
            if (sgPartido is null) { reason = "missing_field"; return false; }
            if (!int.TryParse(csv.Turno, out var turno)) { reason = "invalid_turno"; return false; }
            if (!TseParsing.TryParseMoney(csv.ValorReceita, out var valor)) { reason = "invalid_valor"; return false; }

            var (cpf, cnpj) = TseParsing.SplitCpfCnpj(csv.CpfCnpjDoador);

            row = new ReceitaRow(idCandidato, csv.AnoEleicao, turno.ToString(), Sq(csv)!, sgPartido,
                TseParsing.NullIfBlank(csv.FonteReceita), TseParsing.NullIfTseEmpty(csv.OrigemReceita),
                TseParsing.NullIfBlank(csv.NaturezaReceita), TseParsing.NullIfBlank(csv.EspecieReceita), cnpj, cpf,
                TseParsing.NullIfTseEmpty(csv.NomeDoador), TseParsing.NullIfTseEmpty(csv.NomeDoadorRFB),
                TseParsing.NullIfTseEmpty(csv.NomeMunicipioDoador), TseParsing.ParseDateTimeUtc(csv.DataReceita),
                TseParsing.NullIfTseEmpty(csv.DescricaoReceita), valor);
            return true;
        }

        protected override void Write(NpgsqlBinaryImporter w, ReceitaRow r)
        {
            w.WriteGuid(r.IdCandidato);
            w.WriteInt(r.Ano);
            w.WriteText(r.Turno, 2);
            w.WriteText(r.Sq, 50);
            w.WriteText(r.SgPartido, 50);
            w.WriteText(r.Fonte, 150);
            w.WriteText(r.Origem, 250);
            w.WriteText(r.Natureza, 250);
            w.WriteText(r.Especie, 250);
            w.WriteText(r.Cnpj, 14);
            w.WriteText(r.Cpf, 11);
            w.WriteText(r.Nome, 255);
            w.WriteText(r.NomeRfb, 255);
            w.WriteText(r.Municipio, 100);
            w.WriteTimestampUtc(r.Data);
            w.WriteText(r.Descricao);
            w.WriteNumeric(r.Valor);
        }
    }
}
