using OpenCand.API.Model;
using OpenCand.API.Repository;
using OpenCand.Core.Models;
using OpenCand.Core.Utils;
using OpenCand.Repository;

namespace OpenCand.API.Services
{
    public class OpenCandService
    {
        private readonly OpenCandRepository openCandRepository;
        private readonly CandidatoRepository candidatoRepository;
        private readonly BemCandidatoRepository bemCandidatoRepository;
        private readonly DespesaReceitaRepository despesaReceitaRepository;
        private readonly IConfiguration configuration;
        private readonly FotoLocator fotoLocator;
        private readonly PopularityTracker popularityTracker;
        private readonly ILogger<OpenCandService> logger;

        public OpenCandService(
            OpenCandRepository openCandRepository,
            CandidatoRepository candidatoRepository,
            BemCandidatoRepository bemCandidatoRepository,
            DespesaReceitaRepository despesaReceitaRepository,
            FotoLocator fotoLocator,
            PopularityTracker popularityTracker,
            IConfiguration configuration,
            ILogger<OpenCandService> logger)
        {
            this.openCandRepository = openCandRepository;
            this.candidatoRepository = candidatoRepository;
            this.bemCandidatoRepository = bemCandidatoRepository;
            this.despesaReceitaRepository = despesaReceitaRepository;
            this.fotoLocator = fotoLocator;
            this.popularityTracker = popularityTracker;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<OpenCandStats> GetOpenCandStatsAsync()
        {
            logger.LogInformation("Getting OpenCand stats");
            
            return await openCandRepository.GetOpenCandStatsAsync();
        }

        public async Task<DataAvailabilityStats> GetDataAvailabilityStatsAsync()
        {
            logger.LogInformation("Getting data availability stats");

            var stats = await openCandRepository.GetDataAvailabilityAsync();

            return stats;
        }

        public async Task<DatabaseTechStats> GetDatabaseTechStatsAsync()
        {
            logger.LogInformation("Getting database tech stats");

            var stats = await openCandRepository.GetDatabaseTechStatsAsync();

            stats.Tables = stats.Tables.OrderBy(t => t.Name).ToList();
            stats.MaterializedViews = stats.MaterializedViews.OrderBy(mv => mv.Name).ToList();

            return stats;
        }

        public async Task<CandidatoSearchResult> SearchCandidatosAsync(string query)
        {
            logger.LogInformation($"Searching candidatos with query: {query}");

            return new CandidatoSearchResult()
            {
                Candidatos = await candidatoRepository.SearchCandidatosAsync(query) ?? new List<Candidato>()
            };
        }

        public async Task<Guid> GetRandomCandidato()
        {
            logger.LogInformation("Getting random candidato");

            return await candidatoRepository.GetRandomCandidatoIdAsync()
                ?? throw new KeyNotFoundException("No candidatos available.");
        }

        public async Task<Candidato> GetCandidatoAsync(Guid idcandidato)
        {
            var detail = await candidatoRepository.GetCandidatoDetailAsync(idcandidato);

            if (detail == null)
            {
                throw new KeyNotFoundException($"Candidato with ID {idcandidato} not found.");
            }
            if (detail.Eleicoes.Count == 0)
            {
                throw new KeyNotFoundException($"CandidatoMapping for ID {idcandidato} not found.");
            }

            var result = detail.Candidato;

            // Counted in memory and flushed to the database by PopularityFlushService.
            popularityTracker.Increment(result.IdCandidato);

            var eleicoes = detail.Eleicoes.OrderByDescending(e => e.Ano).ToList();
            var lastEleicao = eleicoes[0];

            result.FotoUrl = fotoLocator.GetFotoUrl(lastEleicao.Ano, lastEleicao.SiglaUF, lastEleicao.SqCandidato)!;
            result.Eleicoes = eleicoes;
            result.CandidatoExt = detail.CandidatoExt.OrderByDescending(ce => ce.Ano).ToList();

            logger.LogDebug("Found Candidato {IdCandidato} ({Nome})", result.IdCandidato, result.Nome);

            return result;
        }

        public async Task<BemCandidatoResult> GetBemCandidatoById(Guid idcandidato)
        {
            var result = await bemCandidatoRepository.GetBemCandidatoAsync(idcandidato);
            if (result == null)
            {
                result = new List<BemCandidato>();
            }

            logger.LogInformation($"Found {result.Count} bens for Candidato ID {idcandidato}");

            return new BemCandidatoResult()
            {
                Bens = result.OrderByDescending(r => r.TipoBem).ThenByDescending(r => r.Valor).ToList()
            };
        }

        public async Task<RedeSocialResult> GetCandidatoRedeSocialById(Guid idcandidato)
        {
            logger.LogInformation($"Getting redes sociais for Candidato ID {idcandidato}");

            var result = await candidatoRepository.GetCandidatoRedeSocialById(idcandidato);
            if (result == null)
            {
                result = new List<RedeSocial>();
            }

            logger.LogDebug($"Found {result.Count} redes sociais for Candidato ID {idcandidato}");

            return new RedeSocialResult()
            {
                RedesSociais = result.OrderByDescending(r => r.Ano).ToList()
            };
        }

        public async Task<CpfRevealResult> GetCandidatoCpfById(Guid idcandidato)
        {
            logger.LogInformation($"Getting CPF for Candidato ID {idcandidato}");

            var result = await candidatoRepository.GetCandidatoCpfAsync(idcandidato);
            if (result == null)
            {
                return new CpfRevealResult();
            }

            return new CpfRevealResult()
            {
                Cpf = result
            };
        }

        public async Task<DespesasResult> GetDespesasByIdAndYear(Guid idcandidato, int? ano = null, int? page = null, int? pageSize = null)
        {
            var paging = Paging.Resolve(page, pageSize);
            logger.LogInformation("Getting despesas for Candidato ID {IdCandidato} (ano={Ano}, page={Page}, pageSize={PageSize})", idcandidato, ano, page, pageSize);

            var (items, total) = await despesaReceitaRepository.GetDespesasAsync(idcandidato, ano, paging);
            items.ForEach(d => d.CpfFornecedor = CpfMasking.MaskCpf(d.CpfFornecedor));

            return new DespesasResult()
            {
                Despesas = items,
                Page = paging.Page,
                PageSize = paging.Enabled ? paging.PageSize : items.Count,
                Total = total
            };
        }

        public async Task<ReceitaResult> GetReceitasByIdAndYear(Guid idcandidato, int? ano = null, int? page = null, int? pageSize = null)
        {
            var paging = Paging.Resolve(page, pageSize);
            logger.LogInformation("Getting receitas for Candidato ID {IdCandidato} (ano={Ano}, page={Page}, pageSize={PageSize})", idcandidato, ano, page, pageSize);

            var (items, total) = await despesaReceitaRepository.GetReceitasAsync(idcandidato, ano, paging);
            items.ForEach(r => r.CpfDoador = CpfMasking.MaskCpf(r.CpfDoador));

            return new ReceitaResult()
            {
                Receitas = items,
                Page = paging.Page,
                PageSize = paging.Enabled ? paging.PageSize : items.Count,
                Total = total
            };
        }

        public async Task<FinanceResumoResult> GetDespesasResumo(Guid idcandidato)
        {
            return new FinanceResumoResult { Resumo = await despesaReceitaRepository.GetDespesasResumoAsync(idcandidato) };
        }

        public async Task<FinanceResumoResult> GetReceitasResumo(Guid idcandidato)
        {
            return new FinanceResumoResult { Resumo = await despesaReceitaRepository.GetReceitasResumoAsync(idcandidato) };
        }
    }
}
