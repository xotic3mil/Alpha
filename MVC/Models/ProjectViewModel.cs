using Business.Dtos;
using Domain.Models;

namespace MVC.Models
{
    public class ProjectViewModel
    {

        public IEnumerable<Status> Statuses { get; set; } = new List<Status>();
        public IEnumerable<Service> Services { get; set; } = new List<Service>();
        public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
        public IEnumerable<User>? Users { get; set; } = new List<User>();
        public List<UserViewModel> ProjectUsers { get; set; } = new List<UserViewModel>();
        public IEnumerable<Project> Projects { get; set; } = new List<Project>();
        public ProjectRegForm Form { get; set; } = new ProjectRegForm();
    }

    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }
}
