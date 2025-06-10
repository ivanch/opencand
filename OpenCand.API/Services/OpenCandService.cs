using Microsoft.Extensions.Options;
using OpenCand.API.Config;
using OpenCand.API.Model;
using OpenCand.API.Repository;
using OpenCand.Core.Models;
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
        private readonly FotosSettings fotoSettings;
        private readonly ILogger<OpenCandService> logger;

        public OpenCandService(
            OpenCandRepository openCandRepository,
            CandidatoRepository candidatoRepository,
            BemCandidatoRepository bemCandidatoRepository,
            DespesaReceitaRepository despesaReceitaRepository,
            IOptions<FotosSettings> fotoSettings,
            IConfiguration configuration,
            ILogger<OpenCandService> logger)
        {
            this.openCandRepository = openCandRepository;
            this.candidatoRepository = candidatoRepository;
            this.bemCandidatoRepository = bemCandidatoRepository;
            this.despesaReceitaRepository = despesaReceitaRepository;
            this.fotoSettings = fotoSettings.Value;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<OpenCandStats> GetOpenCandStatsAsync()
        {
            return await openCandRepository.GetOpenCandStatsAsync();
        }

        public async Task<DataAvailabilityStats> GetDataAvailabilityStatsAsync()
        {
            var stats = await openCandRepository.GetDataAvailabilityAsync();

            return stats;
        }

        public async Task<CandidatoSearchResult> SearchCandidatosAsync(string query)
        {
            return new CandidatoSearchResult()
            {
                Candidatos = await candidatoRepository.SearchCandidatosAsync(query)
            };
        }

        public async Task<Candidato> GetCandidatoAsync(Guid idcandidato)
        {
            var result = await candidatoRepository.GetCandidatoAsync(idcandidato);
            var eleicoes = await candidatoRepository.GetCandidatoMappingById(idcandidato);
            var candidatoExt = await candidatoRepository.GetCandidatoExtById(idcandidato);

            if (result == null)
            {
                throw new KeyNotFoundException($"Candidato with ID {idcandidato} not found.");
            }
            if (eleicoes == null || eleicoes.Count == 0)
            {
                throw new KeyNotFoundException($"CandidatoMapping for ID {idcandidato} not found.");
            }

            foreach (var eleicao in eleicoes)
            {
                eleicao.Partido = await candidatoRepository.GetPartidoBySigla(eleicao.Sgpartido);
            }

            var lastEleicao = eleicoes.OrderByDescending(e => e.Ano).First();

            result.FotoUrl = $"{fotoSettings.ApiBasePath}/foto_cand{lastEleicao.Ano}_{lastEleicao.SiglaUF}_div/F{lastEleicao.SiglaUF}{lastEleicao.SqCandidato}_div.jpg";
            result.Eleicoes = eleicoes.OrderByDescending(e => e.Ano).ToList();
            result.CandidatoExt = candidatoExt.OrderByDescending(ce => ce.Ano).ToList();

            return result;
        }

        public async Task<BemCandidatoResult> GetBemCandidatoById(Guid idcandidato)
        {
            var result = await bemCandidatoRepository.GetBemCandidatoAsync(idcandidato);
            if (result == null)
            {
                result = new List<BemCandidato>();
            }

            return new BemCandidatoResult()
            {
                Bens = result.OrderByDescending(r => r.TipoBem).ThenByDescending(r => r.Valor).ToList()
            };
        }

        public async Task<RedeSocialResult> GetCandidatoRedeSocialById(Guid idcandidato)
        {
            var result = await candidatoRepository.GetCandidatoRedeSocialById(idcandidato);
            if (result == null)
            {
                result = new List<RedeSocial>();
            }

            return new RedeSocialResult()
            {
                RedesSociais = result.OrderByDescending(r => r.Ano).ToList()
            };
        }

        public async Task<CpfRevealResult> GetCandidatoCpfById(Guid idcandidato)
        {
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

        public async Task<DespesasResult> GetDespesasByIdAndYear(Guid idcandidato)
        {
            var result = await despesaReceitaRepository.GetDespesasByCandidatoIdYearAsync(idcandidato);
            if (result == null)
            {
                return new DespesasResult();
            }

            return new DespesasResult()
            {
                Despesas = result.OrderByDescending(d => d.Ano).ThenByDescending(d => d.Valor).ToList()
            };
        }

        public async Task<ReceitaResult> GetReceitasByIdAndYear(Guid idcandidato)
        {
            var result = await despesaReceitaRepository.GetReceitasByCandidatoIdYearAsync(idcandidato);
            if (result == null)
            {
                return new ReceitaResult();
            }

            return new ReceitaResult()
            {
                Receitas = result.OrderByDescending(d => d.Ano).ThenByDescending(d => d.Valor).ToList()
            };
        }
    }
}
