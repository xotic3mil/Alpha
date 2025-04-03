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
        public IEnumerable<User>? Result { get; set; }
    }
}
