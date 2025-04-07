using Business.Dtos;
using Domain.Models;
using System.Threading.Tasks;

namespace Business.Interfaces
{
    public interface ICustomersService
    {

        public Task<CustomerResult<IEnumerable<Customer>>> GetCustomersAsync();
        public Task<CustomerResult<Customer>> GetCustomerByNameAsync(string CompanyName);
        public Task<CustomerResult<Customer>> GetCustomerByIdAsync(Guid id);
        public Task<CustomerResult> CreateCustomerAsync(CustomerRegForm form);
        public Task<CustomerResult> UpdateCustomerAsync(CustomerRegForm form);
        public Task<CustomerResult<Customer>> DeleteCustomerAsync(Guid id);

    }
}
