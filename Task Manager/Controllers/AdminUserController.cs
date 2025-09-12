using Application.Features.Admin;
using Application.Services.DTOs.PorjectDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [ApiController]
    public class AdminUserController : Controller
    {
        private readonly AdminUserService _service;

        public AdminUserController(AdminUserService service)
        {
            _service = service;
        }

        [HttpGet("users/get-all")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var result = await _service.GetAllUsersAsync();

            if (!result.isSucceded)
            {
                return NotFound(new
                {
                    Message = result.message,
                    Errors = result.errors
                });
            }

            return Ok(new { Message = result.message, result.users });
        }

        [HttpPut("users/assign-to-project")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> AssignAsync([FromBody] AssignRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service
                .AssignOnProjectAsync(request);

            if (!result.isSucceded)
            {
                return NotFound(new { Message = result.message, Errors = result.errors });
            }

            return Ok(new { Message = result.message });
        }

        [HttpPut("users/assign-role")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> AssignRoleAsync
            ([FromBody] AssignRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.AssignRoleAsync(request);
            if (!result.isSucceded)
            {
                return BadRequest(new {Errors = result.errors, 
                    Message = result.message});
            }

            return Ok(new {Message = result.message});
        }

        [HttpDelete("users/delete/username={userName}")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> DeleteUserAsync(string userName)
        {
            var result = await _service.DeleteUserAsync(userName);

            if (!result.isSucceded)
            {
                return BadRequest(new
                {
                    Errors = result.errors,
                    Message = result.message
                }); 
            }

            return Ok(new {Message = result.message});
        }
    }
}
