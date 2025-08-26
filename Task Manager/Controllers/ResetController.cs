using Application.Features.Reset;
using Application.Services.DTOs.AuthenticationDTOS;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    public class ResetController : Controller
    {
        private readonly ResetService _service;

        public ResetController(ResetService service)
        {
            _service = service;
        }

        [HttpPost("user/reset")]
        public async Task<IActionResult> ResetAsync
            (ResetRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var resetLink = Url.Action(
                nameof(ResetAsync),
                "Authentication",
                null,
                Request.Scheme,
                Request.Host.ToString());

            var result = await _service
                .ResetAsync(request, resetLink);

            if (!result.isScudded)
            {
                return BadRequest(new
                {
                    Message = result.message,
                    Errors = result.errors
                });
            }

            return Ok(new { Message = result.message });
        }

        [HttpPut("user/update-credentials")]
        public async Task<IActionResult> UpdateCredentialsAsync
            ([FromBody] UpdateRequest request,
            [FromQuery] string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service
                .UpdateCredentialsAsync(request, token);
            if (!result.isSuccede)
            {
                return Conflict(new
                {
                    Errors = result.errors,
                    Message = result.message
                });
            }

            return Ok(new { Message = result.message });
        }
    }
}
