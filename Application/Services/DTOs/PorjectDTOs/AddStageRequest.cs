namespace Application.Services.DTOs.PorjectDTOs
{
    public class AddStageRequest
    {
        public Guid ProjectId { get; set; }
        public string? Title { get; set; }
        public List<Guid> AssignedUserIds { get; set; }
    }
}
