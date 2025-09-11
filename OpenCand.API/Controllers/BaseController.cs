using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenCand.API.Config;

namespace OpenCand.API.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    [Produces("application/json")]
    [EnableRateLimiting(RateLimitingConfig.DefaultPolicy)]
    public class BaseController : Controller
    {

    }
}
