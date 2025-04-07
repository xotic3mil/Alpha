using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Dtos
{
    public class CommentResult<T> : StatusResults
    {
        public T? Result { get; set; }
    }

    public class CommentResult : StatusResults
    {

    }



}
