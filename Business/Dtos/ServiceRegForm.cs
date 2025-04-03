using System.ComponentModel.DataAnnotations;

namespace Business.Dtos;

public class ServiceRegForm
{
    [Required]
    public string ServiceName { get; set; } = null!;
    public string ServiceDescription { get; set; } = null!;
    public decimal Budget { get; set; }



}
