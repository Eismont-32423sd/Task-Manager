using Domain.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DbEntities
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        [MinLength(5)]
        [MaxLength(50)]
        public string? Title { get; set; }
        [MinLength(5)]
        [MaxLength(50)]
        public string? Description { get; set; }
        public List<Stage> Stages { get; set; } = [];
        public ProjectType ProjectType { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
        public List<User> Participants { get; set; } = [];
    }
}
