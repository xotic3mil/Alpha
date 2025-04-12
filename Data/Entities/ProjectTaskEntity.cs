using Domain.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class ProjectTaskEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        public bool IsCompleted { get; set; } = false;

        public decimal? EstimatedHours { get; set; }

        public DateTime? CompletedAt { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual ProjectEntity Project { get; set; }

        public Guid? AssignedToId { get; set; }

        [ForeignKey("AssignedToId")]
        public virtual UserEntity AssignedTo { get; set; }

        [Required]
        public Guid CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserEntity CreatedBy { get; set; }

        public virtual ICollection<TimeEntryEntity> TimeEntries { get; set; }
    }
}
