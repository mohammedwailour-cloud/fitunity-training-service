using Training.Application.Sessions.Interfaces;

namespace Training.Application.Sessions.UseCases
{
    public class DeleteSessionUseCase
    {
        private readonly ISessionRepository _sessionRepository;

        public DeleteSessionUseCase(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<bool> ExecuteAsync(Guid id)
        {
            var session = await _sessionRepository.GetByIdAsync(id);

            if (session == null)
                return false;

            await _sessionRepository.DeleteAsync(id);
            return true;
        }
    }
}