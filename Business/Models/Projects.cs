using Data.Entities;

namespace Business.Models;

public class Projects
{
    public Guid Id { get; set; } 
    public Guid ProjectNumber { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Guid StatusId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }
    public StatusTypes Status { get; set; } = null!;
    public Customers Customers { get; set; } = null!;
    public Services Service { get; set; } = null!;
    public IEnumerable<Users> Users { get; set; } = new List<Users>();

}
