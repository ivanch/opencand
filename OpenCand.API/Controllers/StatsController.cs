using Microsoft.AspNetCore.Mvc;
using OpenCand.API.Services;
using OpenCand.Core.Models;

namespace OpenCand.API.Controllers
{
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
    }
}
