namespace Domain.Entities
{
    public class Commit
    {
        public Guid Id { get; set; }
        public Guid StageAssignmentStageId { get; set; }
        public Guid StageAssignmentUserId { get; set; }
        public string? Message { get; set; }
        public DateOnly CommitDate { get; set; }
    }
}
