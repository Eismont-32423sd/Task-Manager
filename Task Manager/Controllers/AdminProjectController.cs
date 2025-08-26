using Application.Features.Admin;
using Application.Services.DTOs.PorjectDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [ApiController]
    public class AdminProjectController : Controller
    {
        private readonly AdminProjectService _service;

        public AdminProjectController(AdminProjectService service)
        {
            _service = service;
        }

        [HttpGet("projects/get-all")]
        [Authorize(Roles = "TeamLead")]
        public async Task<IActionResult> GetAllProjectsAsync()
        {
            var result = await _service.GetAllProjectsAsync();

            if (!result.isSucceded)
            {
                return NotFound(new
                {
                    Message = result.message,
                    Errors = result.errors
                });
            }

            return Ok(new { Message = result.message, result.projects });
        }

        [HttpPost("prject/create")]
        [Authorize(Roles = "TeamLead, Manager")]
        public async Task<IActionResult> CreateProjectAsync(CreateProjectRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.CreateProjectAsync(request);
            if (!result.isSucceded)
            {
                return BadRequest(new
                {
                    Message = result.message,
                    Errors = result.errors
                });
            }

            return Ok(new { Message = result.message });
        }

        [HttpGet("project/get/projectid={projectId}")]
        [Authorize]
        public async Task<IActionResult> GetProjectByIdAsync(Guid projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.GetProjectByIdAsync(projectId);
            if (!result.isSucceded)
            {
                return NotFound(new { Message = result.message, Errors = result.errors });
            }

            return Ok(new { Message = result.message, result.project });
        }

        [HttpPut("project/change/porjectTitle={projectTitle}")]
        [Authorize(Roles = "TeamLead, Manager")]
        public async Task<IActionResult> UpdateProjectAsync
            (string projectTitle, [FromBody] UpdateProjectRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service
                .UpdateProjectAsync(projectTitle, request);
            if (!result.isSucceded)
            {
                return NotFound(new { Message = result.message, Errors = result.errors });
            }

            return Ok(new { Message = result.message });
        }
    }
}
