using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("Users")]
public class UserEntity : IdentityUser<Guid>
{
    [ProtectedPersonalData]
    public string FirstName { get; set; } = null!;

    [ProtectedPersonalData]
    public string LastName { get; set; } = null!;

    public string? Title { get; set; } = null!;

    [MaxLength(20)]
    public override string? PhoneNumber { get; set; } = null!;

    [Url]
    [MaxLength(2083)]
    public string? AvatarUrl { get; set; }

    public ICollection<ProjectEntity>? Projects { get; set; } = new List<ProjectEntity>();


}
