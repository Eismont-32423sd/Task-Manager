using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DbEntities
{
    public class Commit
    {
        public Guid Id { get; set; }
        public Guid StageAssignmentStageId { get; set; }
        public Guid StageAssignmentUserId { get; set; }
        [MinLength(5)]
        [MaxLength(256)]
        public string? Message { get; set; }
        public DateOnly CommitDate { get; set; }
    }
}
