using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WEB.Controollers
{
    [Route("api/antiforgerytoken")]
    [ApiController]
    public class AntiforgeryController : ControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiforgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        [HttpGet]
        public IActionResult GetAntiforgeryToken()
        {
            var token = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
            return Ok(token);
        }
    }

}
