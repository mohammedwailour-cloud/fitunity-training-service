using Training.Application.Events.DTOs;
using Training.Domain.Entities;

namespace Training.Application.Events.Mappers;

public static class EventMapper
{
    public static EventDto ToDto(Event ev, Space? space = null)
    {
        Space? resolvedSpace = space ?? ev.Space;

        return new EventDto
        {
            Id = ev.Id,
            Titre = ev.Titre,
            Description = ev.Description ?? string.Empty,
            DateDebut = ev.DateDebut,
            DateFin = ev.DateFin,
            Capacite = ev.Capacite,
            SpaceId = ev.SpaceId,
            SpaceName = resolvedSpace?.Name ?? string.Empty,
            SpaceType = resolvedSpace?.Type ?? default
        };
    }

    public static List<EventDto> ToDtoList(IEnumerable<Event> events)
    {
        return events.Select(ev => ToDto(ev)).ToList();
    }
}
