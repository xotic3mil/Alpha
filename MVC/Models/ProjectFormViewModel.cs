namespace MVC.Models
{
    public class ProjectFormViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int StatusId { get; set; }
        public int CustomerId { get; set; }
        public int ServiceId { get; set; }
    }
}
