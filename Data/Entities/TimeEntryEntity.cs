using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class TimeEntryEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [Range(0.25, 24)]
    public double Hours { get; set; }
 
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsBillable { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal HourlyRate { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    public virtual ProjectEntity? Project { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual UserEntity? User { get; set; }

    public Guid? TaskId { get; set; }

    [ForeignKey("TaskId")]
    public virtual ProjectTaskEntity? Task { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public decimal BillableAmount => IsBillable ? (decimal)Hours * HourlyRate : 0;
}
