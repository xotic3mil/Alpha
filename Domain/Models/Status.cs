using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class Status
{
    public Guid Id { get; set; }
    [Required]
    public string StatusName { get; set; } = null!;
    [Required]
    public string Description { get; set; } = null!;
    [Required]
    public string ColorCode { get; set; } = null!;

}
