using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class RepositoryResult<T>
    {
        public bool Succeeded { get; set; }
        public int StatusCode { get; set; }
        public string? Error { get; set; }
        public T? Result { get; set; }


    }
}
