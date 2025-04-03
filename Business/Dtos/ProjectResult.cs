using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Dtos
{
    public class ProjectResult<T> : StatusResults
    {
        public T? Result { get; set; }
    }

    public class ProjectResult : StatusResults
    {
       
    }
}
