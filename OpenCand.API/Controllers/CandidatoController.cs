using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.VisualBasic;
using OpenCand.API.Config;
using OpenCand.API.Model;
using OpenCand.API.Services;
using OpenCand.Core.Models;

namespace OpenCand.API.Controllers
{
    [EnableRateLimiting(RateLimitingConfig.DefaultPolicy)]
    public class CandidatoController : BaseController
    {
        private readonly OpenCandService openCandService;

        public CandidatoController(OpenCandService openCandService)
        {
            this.openCandService = openCandService;
        }

        [HttpGet("search")]
        [EnableRateLimiting(RateLimitingConfig.CandidatoSearchPolicy)]
        public async Task<CandidatoSearchResult> CandidatoSearch([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length == 1)
            {
                throw new ArgumentException("Query parameter 'q' cannot be null/empty.", nameof(q));
            }

            return await openCandService.SearchCandidatosAsync(q);
        }

        [HttpGet("{id}")]
        public async Task<Candidato> GetCandidatoById([FromRoute] Guid id)
        {
            return await openCandService.GetCandidatoAsync(id);
        }

        [HttpGet("{id}/bens")]
        public async Task<BemCandidatoResult> GetBensCandidatoById([FromRoute] Guid id)
        {
            return await openCandService.GetBemCandidatoById(id);
        }

        [HttpGet("{id}/rede-social")]
        public async Task<RedeSocialResult> GetCandidatoRedeSocialById([FromRoute] Guid id)
        {
            return await openCandService.GetCandidatoRedeSocialById(id);
        }

        [HttpGet("{id}/reveal-cpf")]
        [EnableRateLimiting(RateLimitingConfig.CpfRevealPolicy)]
        public async Task<CpfRevealResult> GetCandidatoCpfById([FromRoute] Guid id)
        {
            var rnd = new Random();
            var randomWait = rnd.Next(1000, 3000);
            await Task.Delay(randomWait);
            return await openCandService.GetCandidatoCpfById(id);
        }

        [HttpGet("{id}/despesas")]
        public async Task<DespesasResult> GetCandidatoDespesas([FromRoute] Guid id)
        {
            return await openCandService.GetDespesasByIdAndYear(id);
        }

        [HttpGet("{id}/receitas")]
        public async Task<ReceitaResult> GetCandidatoReceitas([FromRoute] Guid id)
        {
            return await openCandService.GetReceitasByIdAndYear(id);
        }
    }
}
