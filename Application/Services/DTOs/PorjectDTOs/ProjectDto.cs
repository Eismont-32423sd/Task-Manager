using Application.Services.DTOs.AuthenticationDTOS;

namespace Application.Services.DTOs.PorjectDTOs
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<UserDto> Participants { get; set; } = [];
    }
}
