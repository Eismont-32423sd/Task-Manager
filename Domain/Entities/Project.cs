namespace Domain.Entities
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ProjectType ProjectType { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
        public List<User> Participants { get; set; } = [];
        public Guid? OwnerId { get; set; }
    }
}
