using Application.Services.DTOs.PorjectDTOs;
using Domain.Entities;

namespace Application.Features
{
    public class ServiceResult<T>
    {
        public bool isSucceded { get; set; }
        public string? message { get; set; }
        public IEnumerable<string>? errors { get; set; }
        public T? Data { get; set; } 
    }
}
