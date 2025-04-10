namespace MVC.Models
{
    public class NotificationViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Message { get; set; }
        public string Type { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid? RelatedEntityId { get; set; }
    }
}
