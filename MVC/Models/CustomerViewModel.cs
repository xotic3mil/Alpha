using Business.Dtos;
using Domain.Models;
using System.Collections.Generic;

namespace MVC.Models
{
    public class CustomerViewModel
    {
        public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
        public CustomerRegForm Form { get; set; } = new CustomerRegForm();
    }
}