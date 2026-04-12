using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Application.Coachs.DTOs;

public class CreateCoachDto
{
    public string Nom { get; set; }
    public string Email { get; set; }
    public Guid ActivityId { get; set; }
}