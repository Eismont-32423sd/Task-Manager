using Application.Features.User;
using Application.Services.DTOs.PorjectDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Task_Manager.Controllers
{
    [ApiController]
    public class UserProjectController : BaseController
    {
        private readonly UserProjectService _service;
        public UserProjectController(UserProjectService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("project/commit")]
        public async Task<IActionResult>
            CommitAsync([FromBody] CommitRequest request)
        {
            return await HandleServiceCallAsync(() => _service.AddCommitAsync(request));
        }

        [Authorize]
        [HttpGet("project/get-all-commits")]
        public async Task<IActionResult> GetAllCommitsAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token: missing user id");

            var userId = Guid.Parse(userIdClaim);

            return await HandleServiceCallAsync(() => _service.GetAllCommitsAsync(userId));
        }

    }
}
