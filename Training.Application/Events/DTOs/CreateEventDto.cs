using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Application.Events.DTOs
{


    public class CreateEventDto
    {
        public string Titre { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int Capacite { get; set; }
    }
}
