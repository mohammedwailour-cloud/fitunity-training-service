using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Domain.Exceptions
{
  

    public class DuplicateReservationException : Exception
    {
        public DuplicateReservationException()
            : base("User already reserved this session")
        {
        }
    }
}
