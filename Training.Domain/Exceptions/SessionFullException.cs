using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Domain.Exceptions
{
    public class SessionFullException : Exception
    {
        public SessionFullException()
            : base("Session is full")
        {
        }
    }
}
