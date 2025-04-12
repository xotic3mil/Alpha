using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;


public class TimeEntry
{
    public Guid Id { get; set; }
    [Required]
    public DateTime Date { get; set; }

    [Required]
    [Range(0.25, 24, ErrorMessage = "Hours must be between 0.25 and 24")]
    public double Hours { get; set; }

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    public bool IsBillable { get; set; }

    [Range(0, 10000, ErrorMessage = "Hourly rate must be between 0 and 10,000")]
    public decimal HourlyRate { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public Guid? TaskId { get; set; }

    public ProjectTask? Task { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public decimal BillableAmount => IsBillable ? (decimal)Hours * HourlyRate : 0;
}