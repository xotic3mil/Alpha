using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class ProjectEditViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Project name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public Guid StatusId { get; set; }

        [Required(ErrorMessage = "Service is required")]
        public Guid ServiceId { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; } 
        public string ImageUrl { get; set; }
    }
}
