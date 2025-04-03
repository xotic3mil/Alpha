using System.ComponentModel.DataAnnotations;


namespace Business.Dtos;

public class ProjectRegForm
{
    [Required(ErrorMessage = "Project name is required.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "StartDate is required.")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "EndDate is required.")]
    public DateOnly EndDate { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public Guid StatusId { get; set; }

    [Required(ErrorMessage = "Customer is required.")]
    public Guid CustomerId { get; set; }

    [Required(ErrorMessage = "Service is required.")]
    public Guid ServiceId { get; set; }


}
