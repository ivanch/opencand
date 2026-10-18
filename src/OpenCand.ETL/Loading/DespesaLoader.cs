using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenCand.Config;
using OpenCand.ETL.Database;
using OpenCand.ETL.Parsing;
using OpenCand.Parser.Models;

namespace OpenCand.ETL.Loading
{
    public sealed record DespesaRow(Guid IdCandidato, int Ano, string Turno, string Sq, string SgPartido, string? TipoFornecedor,
        string? Cnpj, string? Cpf, string? Nome, string? NomeRfb, string? Municipio, string? TipoDocumento, DateTime? Data,
        string? Descricao, string? Origem, decimal? Valor);

    public sealed class DespesaLoader : StreamingFactLoader<DespesasCSV, DespesaRow>
    {
        public DespesaLoader(NpgsqlDataSource dataSource, SqIndex sqIndex, IOptions<EtlSettings> settings, ILogger<DespesaLoader> logger)
            : base(dataSource, sqIndex, settings, logger) { }

        public override string Dataset => "despesas";
        public override SwapTable Table => SwapTable.Despesas;

        protected override string CopyCommand =>
            "COPY despesas_candidato_new (idcandidato, ano, turno, sqcandidato, sgpartido, tipofornecedor, cnpjfornecedor, cpffornecedor, " +
            "nomefornecedor, nomefornecedorrfb, municipiofornecedor, tipodocumento, datadespesa, descricao, origemdespesa, valor) " +
            "FROM STDIN (FORMAT BINARY)";

        protected override int Ano(DespesasCSV csv) => csv.AnoEleicao;
        protected override string? Sq(DespesasCSV csv) => TseParsing.NullIfBlank(csv.SequencialCandidato);

        protected override bool TryPrepare(DespesasCSV csv, Guid idCandidato, out DespesaRow row, out string reason)
        {
            row = default!;
            reason = string.Empty;

            var sgPartido = TseParsing.NullIfBlank(csv.SiglaPartido);
            if (sgPartido is null) { reason = "missing_field"; return false; }
            if (!int.TryParse(csv.Turno, out var turno)) { reason = "invalid_turno"; return false; }
            if (!TseParsing.TryParseMoney(csv.ValorDespesaContratada, out var valor)) { reason = "invalid_valor"; return false; }

            var (cpf, cnpj) = TseParsing.SplitCpfCnpj(csv.CpfCnpjFornecedor);

            row = new DespesaRow(idCandidato, csv.AnoEleicao, turno.ToString(), Sq(csv)!, sgPartido,
                TseParsing.NullIfTseEmpty(csv.TipoFornecedor), cnpj, cpf,
                TseParsing.NullIfTseEmpty(csv.NomeFornecedor), TseParsing.NullIfTseEmpty(csv.NomeFornecedorRFB),
                TseParsing.NullIfTseEmpty(csv.NomeMunicipioFornecedor), TseParsing.NullIfTseEmpty(csv.TipoDocumento),
                TseParsing.ParseDateTimeUtc(csv.DataDespesa), TseParsing.NullIfTseEmpty(csv.DescricaoDespesa),
                TseParsing.NullIfTseEmpty(csv.OrigemDespesa), valor);
            return true;
        }

        protected override void Write(NpgsqlBinaryImporter w, DespesaRow r)
        {
            w.WriteGuid(r.IdCandidato);
            w.WriteInt(r.Ano);
            w.WriteText(r.Turno, 2);
            w.WriteText(r.Sq, 50);
            w.WriteText(r.SgPartido, 50);
            w.WriteText(r.TipoFornecedor, 150);
            w.WriteText(r.Cnpj, 14);
            w.WriteText(r.Cpf, 11);
            w.WriteText(r.Nome, 255);
            w.WriteText(r.NomeRfb, 255);
            w.WriteText(r.Municipio, 100);
            w.WriteText(r.TipoDocumento, 50);
            w.WriteTimestampUtc(r.Data);
            w.WriteText(r.Descricao);
            w.WriteText(r.Origem);
            w.WriteNumeric(r.Valor);
        }
    }
}
