using Application.Features;
using Application.Services.DTOs.PorjectDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [ApiController]
    public class ProjectController : Controller
    {
        private readonly ProjectService _projectService;
        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet("get/all-users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var result = await _projectService.GetAllUsersAsync();

            if (!result.isSucceded)
            {
                return NotFound(new {Message = result.message, 
                    Errors = result.errors});
            }

            return Ok(new {Message = result.message, result.users});
        }

        [HttpGet("get/all-projects")]
        public async Task<IActionResult> GetAllProjectsAsync()
        {
            var result = await _projectService.GetAllProjectsAsync();

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

        [HttpPost("create/new-porject")]
        public async Task<IActionResult> CreateProjectAsync(CreateProjectRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _projectService.CreateProjectAsync(request);
            if (!result.isSucceded)
            {
                return BadRequest(new {Message = result.message, 
                    Errors = result.errors});
            }

            return Ok(new { Message = result.message });
        }

        [HttpGet("project/get-projectid={projectId}")]
        public async Task<IActionResult> GetProjectByIdAsync(Guid projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _projectService.GetProjectByIdAsync(projectId);
            if (!result.isSucceded)
            {
                return NotFound(new {Message =result.message, Errors = result.errors});
            }

            return Ok(new { Message = result.message, result.project });
        }

        [HttpPut("project/change-porjectTitle={projectTitle}")]
        public async Task<IActionResult> UpdateProjectAsync
            (string projectTitle, [FromBody] UpdateProjectRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _projectService
                .UpdateProjectAsync(projectTitle, request);
            if (!result.isSucceded)
            {
                return NotFound(new { Message = result.message, Errors = result.errors });
            }

            return Ok(new { Message = result.message });
        }

        [HttpPost("project/assign")]
        public async Task<IActionResult> AssignAsync(AssignRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _projectService
                .AssignOnProjectAsync( request);

            if (!result.isSucceded)
            {
                return NotFound(new { Message = result.message, Errors = result.errors });
            }

            return Ok(new { Message = result.message });
        }
    }
}
