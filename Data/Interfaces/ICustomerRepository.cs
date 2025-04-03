using Data.Entities;
using Domain.Models;

namespace Data.Interfaces;

public interface ICustomerRepository : IBaseRepository<CustomerEntity, Customer>
{

}
