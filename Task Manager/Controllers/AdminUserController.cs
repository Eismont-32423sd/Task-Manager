using Application.Features.Admin;
using Application.Services.DTOs.PorjectDTOs;
using Domain.Entities.DbEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [ApiController]
    public class AdminUserController : BaseController
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
            return await HandleServiceCallAsync(() =>  _service.GetAllUsersAsync());
        }

        [HttpPut("users/assign-to-project")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> AssignAsync([FromBody] AssignRequest request)
        {
            return await HandleServiceCallAsync(() => _service.AssignOnProjectAsync(request));
        }

        [HttpPut("users/assign-role")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> AssignRoleAsync
            ([FromBody] AssignRoleRequest request)
        {
            return await HandleServiceCallAsync(() => _service.AssignRoleAsync(request));
        }

        [HttpDelete("users/delete/username={userName}")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> DeleteUserAsync(string userName)
        {
            return await HandleServiceCallAsync(() => _service.DeleteUserAsync(userName));
        }
    }
}
