using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
