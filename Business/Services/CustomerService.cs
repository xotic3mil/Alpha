using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;
using System.Diagnostics;



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


    public async Task<CustomerResult> CreateCustomerAsync(CustomerRegForm form)
    {
        if (form == null)
            return new CustomerResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };
        var projectEntity = form.MapTo<CustomerEntity>();

        var result = await _customerRepository.CreateAsync(projectEntity);

        return result.Succeeded
            ? new CustomerResult { Succeeded = true, StatusCode = 201 }
            : new CustomerResult { Succeeded = false, StatusCode = 500, Error = result.Error };
    }

    public async Task<CustomerResult<Customer>> DeleteCustomerAsync(Guid id)
    {
        var CustomerResponse = await _customerRepository.GetAsync(x => x.Id == id);

        if (!CustomerResponse.Succeeded || CustomerResponse.Result == null)
        {
            return new CustomerResult<Customer> { Succeeded = false, StatusCode = 404, Error = $"customer with ID '{id}' not found." };
        }

        var customerEntity = CustomerResponse.Result.MapTo<CustomerEntity>();
        var deleteResponse = await _customerRepository.DeleteAsync(customerEntity);

        return deleteResponse.Succeeded
            ? new CustomerResult<Customer> { Succeeded = true, StatusCode = 200 }
            : new CustomerResult<Customer> { Succeeded = false, StatusCode = 500, Error = "Failed to delete customer." };
    }

    public async Task<CustomerResult> UpdateCustomerAsync(CustomerRegForm form)
    {
        if (form == null || form.Id == Guid.Empty)
            return new CustomerResult { Succeeded = false, StatusCode = 400, Error = "Invalid customer data" };

        try
        {
            var result = await _customerRepository.UpdateAsync(form, p => p.Id == form.Id);

            return result.Succeeded
                ? new CustomerResult { Succeeded = true, StatusCode = 200 }
                : new CustomerResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating project: {ex.Message}");
            return new CustomerResult { Succeeded = false, StatusCode = 500, Error = $"An error occurred: {ex.Message}" };
        }
    }


}
