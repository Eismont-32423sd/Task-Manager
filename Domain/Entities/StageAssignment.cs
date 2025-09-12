using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class StageAssignment
    {
        public Guid StageId { get; set; }
        public Guid UserId { get; set; }
        public string? StageTitle { get; set; }
        public Stage Stage { get; set; } 
        public User User { get; set; }   
        public bool IsCompleted { get; set; }
        public List<Commit> Commits { get; set; } = []; 
    }
}
