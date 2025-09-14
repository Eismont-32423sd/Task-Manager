using Application.Features.Admin;
using Application.Services.DTOs.PorjectDTOs;
using Domain.Entities.DbEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [Route("admin/users")]
    public class AdminUserController : BaseController
    {
        private readonly AdminUserService _service;

        public AdminUserController(AdminUserService service)
        {
            _service = service;
        }

        [HttpGet("get-all")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            return await HandleServiceCallAsync(() =>  _service.GetAllUsersAsync());
        }

        [HttpPut("assign-to-project")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> AssignAsync([FromBody] AssignRequest request)
        {
            return await HandleServiceCallAsync(() => _service.AssignOnProjectAsync(request));
        }

        [HttpPut("assign-role")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> AssignRoleAsync
            ([FromBody] AssignRoleRequest request)
        {
            return await HandleServiceCallAsync(() => _service.AssignRoleAsync(request));
        }

        [HttpDelete("delete/username={userName}")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> DeleteUserAsync(string userName)
        {
            return await HandleServiceCallAsync(() => _service.DeleteUserAsync(userName));
        }
    }
}
