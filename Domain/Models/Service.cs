namespace Domain.Models;

public class Service
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = null!;
    public string ServiceDescription { get; set; } = null!;
    public decimal Budget { get; set; }


}
