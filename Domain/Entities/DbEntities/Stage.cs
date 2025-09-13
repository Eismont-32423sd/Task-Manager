using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DbEntities
{
    public class Stage
    {
        public Guid Id { get; set; }
        [MinLength(5)]
        [MaxLength(50)]
        public string? Title { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }
        public List<StageAssignment> Assignments { get; set; } = [];
    }
}
