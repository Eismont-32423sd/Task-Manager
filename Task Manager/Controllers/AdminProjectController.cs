using Application.Features.Admin;
using Application.Services.DTOs.PorjectDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [Route("admin/project")]
    public class AdminProjectController : BaseController
    {
        private readonly AdminProjectService _service;

        public AdminProjectController(AdminProjectService service)
        {
            _service = service;
        }

        [HttpGet("get-all")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> GetAllProjectsAsync()
        {
            return await HandleServiceCallAsync(_service.GetAllProjectsAsync);
        }

        [HttpPost("add/new-project")]
        [Authorize(Roles = "TeamLead, Manager")]
        public async Task<IActionResult> CreateProjectAsync(CreateProjectRequest request)
        {
            return await HandleServiceCallAsync(() => _service.CreateProjectAsync(request));
        }

        [HttpGet("get/projectid={projectId}")]
        [Authorize]
        public async Task<IActionResult> GetProjectByIdAsync(Guid projectId)
        {
            return await HandleServiceCallAsync(() =>
            {
                return _service.GetProjectByIdAsync(projectId);
            });
        }

        [HttpPut("update/porjectTitle={projectTitle}")]
        [Authorize(Roles = "TeamLead, Manager")]
        public async Task<IActionResult> UpdateProjectAsync
            (string projectTitle, [FromBody] UpdateProjectRequest request)
        {
            return await HandleServiceCallAsync(() => _service.UpdateProjectAsync(projectTitle, request));
        }

        [HttpDelete("delete/title={title}")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> DeleteProjectAsync(string title)
        {
            return await HandleServiceCallAsync(() => _service.DeleteProjectAsync(title));
        }

        [HttpPost("add/stages")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> AddStagesAsync(AddStageRequest request)
        {
            return await HandleServiceCallAsync(() => _service.AddStagesToProjectAsync(request));
        }
    }
}
