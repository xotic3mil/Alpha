using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;



namespace Business.Services;

public class CustomerService(ICustomerRepository customerRepository) : ICustomersService
{
    private readonly ICustomerRepository _customerRepository = customerRepository;

    public async Task<CustomerResult<IEnumerable<Customer>>> GetCustomersAsync()
    {
        var result = await _customerRepository.GetAllAsync();
        return result.Succeeded
            ? new CustomerResult<IEnumerable<Customer>> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new CustomerResult<IEnumerable<Customer>> { Succeeded = false, StatusCode = 500, Error = result.Error };

    }

    public async Task<CustomerResult<Customer>> GetCustomerByNameAsync(string CompanyName)
    {
        var result = await _customerRepository.GetAsync(x => x.CompanyName == CompanyName);
        return result.Succeeded
            ? new CustomerResult<Customer> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new CustomerResult<Customer> { Succeeded = false, StatusCode = 500, Error = result.Error };

    }

    public async Task<CustomerResult<Customer>> GetCustomerByIdAsync(Guid id)
    {
        var result = await _customerRepository.GetAsync(x => x.Id == id);
        return result.Succeeded
            ? new CustomerResult<Customer> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new CustomerResult<Customer> { Succeeded = false, StatusCode = 500, Error = result.Error };
    }


    public async Task<CustomerResult> CreateCustomer(CustomerRegForm form)
    {
        var existingCustomer = await _customerRepository.ExistsAsync(x => x.CompanyName == form.CompanyName);
        if (existingCustomer.Result)
            return new CustomerResult { Succeeded = false, StatusCode = 409, Error = "Status already exists." };

        var CustomerEntity = new CustomerEntity { CompanyName = form.CompanyName };
        var result = await _customerRepository.CreateAsync(CustomerEntity);

        return result.Succeeded
            ? new CustomerResult { Succeeded = true, StatusCode = 201 }
            : new CustomerResult { Succeeded = false, StatusCode = 500, Error = "Failed to create status." };
    }


}
