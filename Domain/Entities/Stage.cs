namespace Domain.Entities
{
    public class Stage
    {
        public Guid StageId { get; set; }
        public Guid PrjectId { get; set; }
        public Guid UserId { get; set; }
        public string? PrjectTitle {  get; set; }
        public DateOnly CommitDate { get; set; }
        public string? CommitMessage { get; set; }
        public bool IsCompleted { get; set; }
    }
}
