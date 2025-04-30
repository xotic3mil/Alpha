namespace Domain.Models;

public class TimeEntrySummary
{
    public double TotalHours { get; set; }
    public double BillableHours { get; set; }
    public double NonBillableHours { get; set; }
    public decimal TotalBillableAmount { get; set; }
}
