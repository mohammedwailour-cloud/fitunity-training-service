using Training.Application.Coachs.DTOs;
using Training.Domain.Entities;

namespace Training.Application.Coachs.Mappers;

public static class CoachMapper
{
    public static CoachDto ToDto(Coach coach)
    {
        return new CoachDto
        {
            Id = coach.Id,
            Nom = coach.Nom,
            Email = coach.Email,
            ActivityId = coach.ActivityId
        };
    }

    public static List<CoachDto> ToDtoList(List<Coach> coaches)
    {
        return coaches.Select(c => ToDto(c)).ToList();
    }
}