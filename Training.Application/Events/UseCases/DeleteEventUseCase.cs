using Training.Application.Events.Interfaces;

namespace Training.Application.Events.UseCases
{
    public class DeleteEventUseCase
    {
        private readonly IEventRepository _repository;

        public DeleteEventUseCase(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
