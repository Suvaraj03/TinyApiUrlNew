using Microsoft.AspNetCore.Mvc;
using TinyApiUrl.Services;

namespace TinyApiUrl.Controllers
{
    [ApiController]
    public class RedirectController : ControllerBase
    {
        private readonly IUrlService _service;

        public RedirectController(IUrlService service)
        {
            _service = service;
        }

        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var url = await _service.GetByShortCodeAsync(code);

            if (url == null)
                return NotFound();

            return Redirect(url.OriginalUrl);
        }
    }
}
