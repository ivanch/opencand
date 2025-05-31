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
        private readonly IConfiguration configuration;
        private readonly FotosSettings fotoSettings;
        private readonly ILogger<OpenCandService> logger;

        public OpenCandService(
            OpenCandRepository openCandRepository,
            CandidatoRepository candidatoRepository,
            BemCandidatoRepository bemCandidatoRepository,
            IOptions<FotosSettings> fotoSettings,
            IConfiguration configuration,
            ILogger<OpenCandService> logger)
        {
            this.openCandRepository = openCandRepository;
            this.candidatoRepository = candidatoRepository;
            this.bemCandidatoRepository = bemCandidatoRepository;
            this.fotoSettings = fotoSettings.Value;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<OpenCandStats> GetOpenCandStatsAsync()
        {
            return await openCandRepository.GetOpenCandStatsAsync();
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

            if (result == null)
            {
                throw new KeyNotFoundException($"Candidato with ID {idcandidato} not found.");
            }
            if (eleicoes == null || eleicoes.Count == 0)
            {
                throw new KeyNotFoundException($"CandidatoMapping for ID {idcandidato} not found.");
            }

            var lastEleicao = eleicoes.OrderByDescending(e => e.Ano).First();

            result.FotoUrl = $"{fotoSettings.ApiBasePath}/foto_cand{lastEleicao.Ano}_{lastEleicao.SiglaUF}_div/F{lastEleicao.SiglaUF}{lastEleicao.SqCandidato}_div.jpg";
            result.Eleicoes = eleicoes.OrderByDescending(e => e.Ano).ToList();

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
    }
}
