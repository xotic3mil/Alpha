using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Message { get; set; }
        public string Type { get; set; } = null!;
        public bool IsRead { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool ForAdminsOnly { get; set; }
        public Guid? RecipientId { get; set; }
        public User? Recipient { get; set; }
    }
}
