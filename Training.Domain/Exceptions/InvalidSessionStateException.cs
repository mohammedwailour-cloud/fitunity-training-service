using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Domain.Exceptions
{
   

    public class InvalidSessionStateException : Exception
    {
        public InvalidSessionStateException(string message)
            : base(message)
        {
        }
    }
}
