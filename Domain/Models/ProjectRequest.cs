namespace Domain.Models;

public class ProjectRequest
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string UserAvatarUrl { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ResolutionDate { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }

    public Project Project { get; set; }
    public User User { get; set; }
}
