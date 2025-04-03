using Business.Dtos;
using Business.Models;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Interfaces
{
    public interface ICustomersService
    {

        public Task<CustomerResult<IEnumerable<Customer>>> GetCustomersAsync();
        public Task<CustomerResult<Customer>> GetCustomerByNameAsync(string CompanyName);
        public Task<CustomerResult<Customer>> GetCustomerByIdAsync(Guid id);
        public Task<CustomerResult> CreateCustomer(CustomerRegForm form);

    }
}
