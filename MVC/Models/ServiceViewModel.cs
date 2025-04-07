using Business.Dtos;
using Domain.Models;
using System.Collections.Generic;

namespace MVC.Models
{
    public class ServiceViewModel
    {
        public IEnumerable<Service> Services { get; set; } = new List<Service>();
        public ServiceRegForm Form { get; set; } = new ServiceRegForm();
    }
}