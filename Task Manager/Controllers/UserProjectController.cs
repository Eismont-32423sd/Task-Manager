using Application.Features.User;
using Application.Services.DTOs.PorjectDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [ApiController]
    public class UserProjectController : Controller
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
            var result = await _service.AddCommitAsync(request);

            if (!result.isSucceded)
            {
                return BadRequest(new { Message = result.message, Errors = result.errors });
            }

            return Ok(new { Message = result.message });
        }
    }
}
