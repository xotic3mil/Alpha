using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Data.Entities;

public class NotificationEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    [Required]
    public string Type { get; set; } = null!; 

    [Required]
    public bool IsRead { get; set; } = false;

    public Guid? RelatedEntityId { get; set; } 

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool ForAdminsOnly { get; set; } = false;

    public bool ForProjectManagersOnly { get; set; } = false;

    public Guid? RecipientId { get; set; }

    [ForeignKey("RecipientId")]
    public virtual UserEntity? Recipient { get; set; }
}

