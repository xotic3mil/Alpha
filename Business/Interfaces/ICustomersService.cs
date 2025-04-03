using Business.Dtos;
using Domain.Models;

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
