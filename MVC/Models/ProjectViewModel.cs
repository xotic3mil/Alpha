using Business.Dtos;
using Domain.Models;

namespace MVC.Models
{
    public class ProjectViewModel
    {
        public IEnumerable<Status> Statuses { get; set; } = new List<Status>();
        public IEnumerable<Service> Services { get; set; } = new List<Service>();
        public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
        public IEnumerable<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public IEnumerable<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
        public IEnumerable<User>? Users { get; set; } = new List<User>();
        public IEnumerable<Project> Projects { get; set; } = new List<Project>();
        public List<Guid> UserProjectIds { get; set; } = new List<Guid>();
        public ProjectRegForm Form { get; set; } = new ProjectRegForm();
        public object ProjectRequest { get; set; } = new object();
        public List<Guid> UserPendingRequestProjectIds { get; set; } = new List<Guid>();
        public bool IsUserMember(Guid projectId) => UserProjectIds.Contains(projectId);
        public bool HasUserPendingRequest(Guid projectId) => UserPendingRequestProjectIds.Contains(projectId);
    }
}
