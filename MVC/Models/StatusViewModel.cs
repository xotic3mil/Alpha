using Business.Dtos;
using Domain.Models;

namespace MVC.Models;

public class StatusViewModel
{
    public IEnumerable<Status> Statuses { get; set; } = new List<Status>();

    public StatusTypeRegForm Form { get; set; } = new StatusTypeRegForm();
}
