using System.ComponentModel.DataAnnotations;

namespace Business.Dtos;

public class ServiceRegForm
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Service name is required")]
    public string ServiceName { get; set; } = null!;

    [Required(ErrorMessage = "Service description is required")]
    public string ServiceDescription { get; set; } = null!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be greater than zero")]
    public decimal Budget { get; set; }
}
