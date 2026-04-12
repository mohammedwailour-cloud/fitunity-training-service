using System;
using System.Collections.Generic;
using System.Linq;
using Training.Application.Events.DTOs;
using Training.Domain.Entities;

namespace Training.Application.Events.Mappers
{
    public static class EventMapper
    {
        public static EventDto ToDto(Event ev)
        {
            return new EventDto
            {
                Id = ev.Id,
                Titre = ev.Titre,
                Description = ev.Description,
                Date = ev.Date,
                Capacite = ev.Capacite
            };
        }

        public static List<EventDto> ToDtoList(List<Event> events)
        {
            return events.Select(e => ToDto(e)).ToList();
        }
    }
}
