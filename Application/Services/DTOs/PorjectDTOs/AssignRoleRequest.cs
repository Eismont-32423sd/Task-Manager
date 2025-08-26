using Domain.Entities;

namespace Application.Services.DTOs.PorjectDTOs
{
    public class AssignRoleRequest
    {
        public string? UserName { get; set; }
        public Role Role { get; set; }
    }
}
