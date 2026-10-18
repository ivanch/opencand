using Microsoft.AspNetCore.Mvc;
using OpenCand.API.Model;
using OpenCand.API.Services;
using static OpenCand.API.Model.GetValueSumRequest;

namespace OpenCand.API.Controllers
{
    public class EstatisticaController : BaseController
    {
        private readonly EstatisticaService estatisticaService;

        public EstatisticaController(EstatisticaService estatisticaService)
        {
            this.estatisticaService = estatisticaService;
        }

        [HttpGet("configuration")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<ConfigurationModel> GetConfiguration()
        {
            return await estatisticaService.GetConfigurationModel();
        }

        [HttpGet("enriquecimento")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<List<MaioresEnriquecimento>> GetMaioresEnriquecimentos([FromQuery] GetValueSumRequestFilter requestFilter)
        {
            return await estatisticaService.GetMaioresEnriquecimentos(requestFilter);
        }

        [HttpPost("values-sum")]
        public async Task<List<GetValueSumResponse>> GetValuesSum([FromBody] GetValueSumRequest getValueSumRequest)
        {
            return await estatisticaService.GetValueSum(getValueSumRequest);
        }
    }
}
