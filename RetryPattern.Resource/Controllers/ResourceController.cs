using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RetryPattern.Resource.Controllers
{
    [ApiController]
    [Route("api/resource")]
    public class ResourceController : ControllerBase
    {

        private readonly ILogger<ResourceController> _logger;

        public ResourceController(ILogger<ResourceController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult Get()
        {
            var retryCount = Request.Headers["Retry-Count"];

            if (int.TryParse(retryCount, out int retryCountNumber))
            {
                _logger.LogWarning($"Resource called: {retryCountNumber}");

                if (retryCountNumber < 3)
                {
                    return NotFound();
                }
                else
                {
                    return Ok();
                }
            }

            // Estado inicial
            return NotFound();
            //throw new Exception("Erro Gerado");
        }
    }
}
