using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Application.Exceptions
{


    public class SessionNotFoundException : Exception
    {
        public SessionNotFoundException(Guid id)
            : base($"Session {id} not found")
        {
        }
    }
}
