namespace Domain.Entities
{
    public class Stage
    {
        public Guid Id { get; set; } 
        public string? Title { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }
        public List<StageAssignment> Assignments { get; set; } = [];
    }
}
