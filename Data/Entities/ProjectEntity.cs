using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("Projects")]
public class ProjectEntity

{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;
    [Required]
    [StringLength(800)]
    public string Description { get; set; } = null!;

    [Required]
    public DateOnly StartDate { get; set; }
    [Required]
    public DateOnly EndDate { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    [Required]
    [ForeignKey(nameof(StatusId))]
    public Guid StatusId { get; set; }
    public virtual StatusTypesEntity Status { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Service))]
    public Guid ServiceId { get; set; }
    public virtual ServiceEntity Service { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Customer))]
    public Guid CustomerId { get; set; }
    public virtual CustomerEntity Customer { get; set; } = null!;

    public virtual ICollection<UserEntity>? Users { get; set; } = new List<UserEntity>();

}




