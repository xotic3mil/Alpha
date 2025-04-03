namespace Domain.Models;
public class Project
{
      public Guid Id { get; set; }
      public string Name { get; set; } = null!;
      public string Description { get; set; } = null!;
      public DateOnly StartDate { get; set; }
      public DateOnly EndDate { get; set; }
      public Guid StatusId { get; set; }
      public Status Status { get; set; } = null!;
      public Guid CustomerId { get; set; }
      public Customer Customer { get; set; } = null!;
      public Guid ServiceId { get; set; }
      public Service Service { get; set; } = null!;
      public IEnumerable<User>? Users { get; set; }

}
