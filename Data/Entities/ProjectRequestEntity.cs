using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Data.Entities
{
    [Table("ProjectRequests")]
    public class ProjectRequestEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        public DateTime? ResolutionDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; 

        [StringLength(500)]
        public string? Message { get; set; }

        [ForeignKey("ProjectId")]
        public virtual ProjectEntity Project { get; set; }

        [ForeignKey("UserId")]
        public virtual UserEntity User { get; set; }
    }
}
