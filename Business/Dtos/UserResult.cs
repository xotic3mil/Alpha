using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Dtos
{
    public class UserResult : StatusResults
    {

    }

    public class UserResult<T> : StatusResults
    {
        public T? Result { get; set; }
    }
}
