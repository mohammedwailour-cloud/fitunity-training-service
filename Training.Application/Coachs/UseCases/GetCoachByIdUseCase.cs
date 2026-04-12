using Training.Application.Coachs.DTOs;
using Training.Application.Coachs.Interfaces;
using Training.Application.Coachs.Mappers;

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
            var coach = await _repository.GetByIdAsync(id);

            if (coach == null)
                throw new Exception("Coach not found");

            return CoachMapper.ToDto(coach);
        }
    }
}
