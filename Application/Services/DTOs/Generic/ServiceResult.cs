namespace Application.Services.DTOs.Generic
{
    public class ServiceResult<T>
    {
        public bool IsSucceded { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public T? Data { get; set; }
    }
}
