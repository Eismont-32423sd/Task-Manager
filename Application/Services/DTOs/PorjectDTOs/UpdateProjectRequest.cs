using Domain.Entities.Enums;

namespace Application.Services.DTOs.PorjectDTOs
{
    public class UpdateProjectRequest
    {
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ProjectType ProjectType { get; set; }
        public ProjectStatus Status { get; set; }
    }
}
