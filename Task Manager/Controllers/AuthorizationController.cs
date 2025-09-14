using Application.Features.Authorization;
using Application.Services.DTOs.AuthenticationDTOS;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    public class AuthorizationController : BaseController
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
            return await HandleServiceCallAsync(() => _service.LoginAsync(request));
        }
    }
}
