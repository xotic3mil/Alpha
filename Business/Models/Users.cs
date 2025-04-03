using Domain.Models;

namespace Business.Models
{
    public class Users
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public IEnumerable<Project> Projects { get; set; } = new List<Project>();

    }
}
