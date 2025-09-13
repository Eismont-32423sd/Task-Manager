using Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DbEntities
{
    public class User
    {
        public Guid Id { get; set; }

        [MinLength(5)]
        [MaxLength(50)]
        [Required]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public string? Email { get; set; }
        [Required]
        [MaxLength(50)]
        public string? PasswordHash { get; set; }

        [MaxLength(150)]
        public string? ConfirmationToken { get; set; }
        public bool IsConfirmed { get; set; }
        public Role Role { get; set; }
        public List<Project> Projects { get; set; } = [];
        public Guid StageId { get; set; }
        public List<StageAssignment> StageAssignments { get; set; } = [];
    }
}
