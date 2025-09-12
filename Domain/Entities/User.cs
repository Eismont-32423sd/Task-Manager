namespace Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? ConfirmationToken { get; set; }
        public bool IsConfirmed { get; set; }
        public Role Role { get; set; }
        public List<Project> Projects { get; set; } = [];
        public Guid StageId { get; set; }
        public List<StageAssignment> StageAssignments { get; set; } = [];
    }
}
