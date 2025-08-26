using Application.Features.Authorization;
using Application.Services.DTOs.AuthenticationDTOS;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly AuthorizationService _service;

        public AuthorizationController(AuthorizationService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync
            ([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.LoginAsync(request);

            if (result.isSucceded)
            {
                return Ok(new { Message = result.message, result.token });
            }
            else
            {
                return Conflict(new
                {
                    Message = result.message,
                    Errors = result.errors
                });
            }
        }
    }
}
