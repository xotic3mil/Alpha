namespace Domain.Models;

public class Project
{
      public Guid Id { get; set; }
      public string Name { get; set; } = null!;
      public string Description { get; set; } = null!;
      public DateOnly StartDate { get; set; }
      public DateOnly EndDate { get; set; }
      public Status Status { get; set; } = null!;
      public Customer Customer { get; set; } = null!;
      public Service Service { get; set; } = null!;
      public User User { get; set; } = null!;
    public string? ImageUrl { get; set; }
     

}
