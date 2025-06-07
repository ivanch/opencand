using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    public class DespesaReceitaRepository : BaseRepository
    {
        public DespesaReceitaRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task AddDespesaAsync(Despesa despesa)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO despesas_candidato (
                        idcandidato,
                        ano,
                        turno,
                        sqcandidato,
                        sgpartido,
                        tipofornecedor,
                        cnpjfornecedor,
                        cpffornecedor,
                        nomefornecedor,
                        nomefornecedorrfb,
                        municipiofornecedor,
                        tipodocumento,
                        datadespesa,
                        descricao,
                        origemdespesa,
                        valor
                    ) VALUES (
                        @idCandidato,
                        @ano,
                        @turno,
                        @sqCandidato,
                        @sgPartido,
                        @tipoFornecedor,
                        @cnpjFornecedor,
                        @cpfFornecedor,
                        @nomeFornecedor,
                        @nomeFornecedorRFB,
                        @municipioFornecedor,
                        @tipoDocumento,
                        @dataDespesa,
                        @descricao,
                        @origemdespesa,
                        @valor
                    )", new
                {
                    idCandidato = despesa.IdCandidato,
                    ano = despesa.Ano,
                    turno = despesa.Turno,
                    sqCandidato = despesa.SqCandidato,
                    sgPartido = despesa.SgPartido,
                    tipoFornecedor = despesa.TipoFornecedor,
                    cnpjFornecedor = despesa.CnpjFornecedor,
                    cpfFornecedor = despesa.CpfFornecedor,
                    nomeFornecedor = despesa.NomeFornecedor,
                    nomeFornecedorRFB = despesa.NomeFornecedorRFB,
                    municipioFornecedor = despesa.MunicipioFornecedor,
                    tipoDocumento = despesa.TipoDocumento,
                    dataDespesa = despesa.DataDespesa,
                    descricao = despesa.Descricao,
                    origemdespesa = despesa.OrigemDespesa,
                    valor = despesa.Valor
                });
            }
        }

        public async Task AddReceitaAsync(Receita receita)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO receitas_candidato (
                        idcandidato,
                        ano,
                        turno,
                        sqcandidato,
                        sgpartido,
                        fontereceita,
                        origemreceita,
                        naturezareceita,
                        especiereceita,
                        cnpjdoador,
                        cpfdoador,
                        nomedoador,
                        nomedoadorrfb,
                        municipiodoador,
                        datareceita,
                        descricao,
                        valor
                    ) VALUES (
                        @idCandidato,
                        @ano,
                        @turno,
                        @sqCandidato,
                        @sgPartido,
                        @fonteReceita,
                        @origemReceita,
                        @naturezaReceita,
                        @especieReceita,
                        @cnpjDoador,
                        @cpfDoador,
                        @nomeDoador,
                        @nomeDoadorRFB,
                        @municipioDoador,
                        @dataReceita,
                        @descricao,
                        @valor
                    )", new
                {
                    idCandidato = receita.IdCandidato,
                    ano = receita.Ano,
                    turno = receita.Turno,
                    sqCandidato = receita.SqCandidato,
                    sgPartido = receita.SgPartido,
                    fonteReceita = receita.FonteReceita,
                    origemReceita = receita.OrigemReceita,
                    naturezaReceita = receita.NaturezaReceita,
                    especieReceita = receita.EspecieReceita,
                    cnpjDoador = receita.CnpjDoador,
                    cpfDoador = receita.CpfDoador,
                    nomeDoador = receita.NomeDoador,
                    nomeDoadorRFB = receita.NomeDoadorRFB,
                    municipioDoador = receita.MunicipioDoador,
                    dataReceita = receita.DataReceita,
                    descricao = receita.Descricao,
                    valor = receita.Valor
                });
            }
        }
    }
}
