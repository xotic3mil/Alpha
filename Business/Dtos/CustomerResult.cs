using Domain.Models;


namespace Business.Dtos
{

    public class CustomerResult : StatusResults
    {
        public IEnumerable<Customer>? Result { get; set; }

    }

    public class CustomerResult<T> : StatusResults
    {
        public T? Result { get; set; }
    }

}
