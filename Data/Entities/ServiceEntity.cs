using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("Services")]
public class ServiceEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public string ServiceName { get; set; } = null!;

    [Required]
    [StringLength(800)]
    public string ServiceDescription { get; set; } = null!;

    [Required]
    public decimal Budget { get; set; }

    // To see which Projects are using this service
    public ICollection<ProjectEntity>? Projects { get; set; } = [];
}
