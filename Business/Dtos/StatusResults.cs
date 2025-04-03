using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Dtos
{
    public class StatusResults
    {
        public bool Succeeded { get; set; }
        public int StatusCode { get; set; }
        public string? Error { get; set; }
    
    }
}
