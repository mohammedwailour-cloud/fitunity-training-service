using Training.Application.Coachs.DTOs;
using Training.Application.Coachs.Interfaces;
using Training.Application.Coachs.Mappers;
using Training.Application.Exceptions;
using Training.Domain.Entities;

namespace Training.Application.Coachs.UseCases
{
    public class GetCoachByIdUseCase
    {
        private readonly ICoachRepository _repository;

        public GetCoachByIdUseCase(ICoachRepository repository)
        {
            _repository = repository;
        }

        public async Task<CoachDto> Execute(Guid id)
        {
            Coach? coach = await _repository.GetByIdAsync(id);

            if (coach == null)
                throw new CoachNotFoundException(id);

            return CoachMapper.ToDto(coach);
        }
    }
}
