using Application.Features.Authentication;
using Application.Services.DTOs.AuthenticationDTOS;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [Route("authentication")]
    public class AuthenticationController : BaseController
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
            var confirmationLink = "";
            return await HandleServiceCallAsync(() => 
            _authenticationService.RegisterAsync(request, confirmationLink));
        }
    }
}
