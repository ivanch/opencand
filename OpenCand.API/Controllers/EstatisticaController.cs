using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenCand.API.Config;
using OpenCand.API.Model;
using OpenCand.API.Services;

namespace OpenCand.API.Controllers
{
    [EnableRateLimiting(RateLimitingConfig.DefaultPolicy)]
    public class EstatisticaController : BaseController
    {
        private readonly EstatisticaService estatisticaService;

        public EstatisticaController(EstatisticaService estatisticaService)
        {
            this.estatisticaService = estatisticaService;
        }

        [HttpGet("configuration")]
        public async Task<ConfigurationModel> GetConfiguration()
        {
            return await estatisticaService.GetConfigurationModel();
        }

        [HttpGet("enriquecimento")]
        public async Task<List<MaioresEnriquecimento>> GetMaioresEnriquecimentos()
        {
            return await estatisticaService.GetMaioresEnriquecimentos();
        }

        [HttpPost("values-sum")]
        public async Task<List<GetValueSumResponse>> GetValuesSum([FromBody] GetValueSumRequest getValueSumRequest)
        {
            return await estatisticaService.GetValueSum(getValueSumRequest);
        }
    }
}
