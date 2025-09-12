namespace Application.Services.DTOs.PorjectDTOs
{
    public class CommitRequest
    {
        public string? StageTitle { get; set; }
        public string UserName  { get; set; }
        public string CommitMessage { get; set; }
    }
}
