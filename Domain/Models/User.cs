namespace Domain.Models;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string AvatarUrl { get; set; } = null!;
    public string? RoleName { get; set; }
    public string? Title { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}


