using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class ProjectTask : BaseUtc
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public string Priority { get; set; } = "Medium"; 

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        public Project? Project { get; set; }

        public Guid? AssignedToId { get; set; }

        public User? AssignedTo { get; set; }

        [Required]
        public Guid CreatedById { get; set; }

        public User? CreatedBy { get; set; }

        public decimal? EstimatedHours { get; set; }

        public List<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}
