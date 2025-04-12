namespace Domain.Models;

public class ProjectTaskSummary
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double CompletionPercentage { get; set; }
    public int TotalTimeEntries { get; set; }
    public double TotalTimeSpent { get; set; }
    public double TotalTimeEstimated { get; set; }
}
