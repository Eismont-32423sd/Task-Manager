using Application.Features;
using Application.Services.DTOs.AuthenticationDTOS;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly AuthenticationService _authenticationService;
        public AuthenticationController
            (AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync
            ([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var confirmationLink = Url.Action(
                nameof(ConfirmEmailAsync),
                "Authentication",
                null,
                Request.Scheme,
                Request.Host.ToString());

            var result = await _authenticationService
                .RegisterAsync(request, confirmationLink!);

            if (result.isSucceded)
            {
                return Ok(new {Message = result.message});
            }
            else
            {
                return Conflict(new { Message = result.message, 
                    Errors = result.errors });
            }
        }

        [HttpGet("confirmation")]
        public async Task<IActionResult> ConfirmEmailAsync
            ([FromQuery]ConfirmRequest request, [FromQuery]string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authenticationService
                .ConfirmEmailAsync(request, token);

            if (result.isSucceded)
            {
                return Ok(new { Message = result.message });
            }
            else
            {
                return Conflict(new { Message = result.message, 
                    Errors = result.errors });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync
            ([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authenticationService.LoginAsync(request);

            if (result.isSucceded)
            {
                return Ok(new {Message = result.message});
            }
            else
            {
                return Conflict(new {Message = result.message,
                    Errors = result.errors,
                result.token});
            }
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
                nameof(ConfirmEmailAsync),
                "Authentication",
                null,
                Request.Scheme,
                Request.Host.ToString());

            var result = await _authenticationService
                .ResetAsync(request, resetLink);

            if (!result.isScudded)
            {
                return BadRequest(new { Message = result.message, 
                    Errors = result.errors });
            }

            return Ok(new { Message = result.message });
        }

        [HttpPut("user/update-credentials")]
        public async Task<IActionResult> UpdateCredentialsAsync
            ([FromBody]UpdateRequest request, 
            [FromQuery] string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authenticationService
                .UpdateCredentialsAsync(request, token);
            if (!result.isSuccede)
            {
                return Conflict(new { Errors = result.errors, 
                    Message = result.message });
            }

            return Ok(new {Message = result.message});
        }
    }
}
