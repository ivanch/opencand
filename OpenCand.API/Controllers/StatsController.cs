using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenCand.API.Config;
using OpenCand.API.Services;
using OpenCand.Core.Models;

namespace OpenCand.API.Controllers
{
    [EnableRateLimiting(RateLimitingConfig.DefaultPolicy)]
    public class StatsController : BaseController
    {
        private readonly OpenCandService openCandService;

        public StatsController(OpenCandService openCandService)
        {
            this.openCandService = openCandService;
        }

        [HttpGet()]
        public async Task<OpenCandStats> GetStats()
        {
            return await openCandService.GetOpenCandStatsAsync();
        }

        [HttpGet("data-availability")]
        public async Task<DataAvailabilityStats> GetDataAvailabilityStats()
        {
            return await openCandService.GetDataAvailabilityStatsAsync();
        }
    }
}

