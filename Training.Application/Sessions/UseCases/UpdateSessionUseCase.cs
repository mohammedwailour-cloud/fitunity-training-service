using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;

namespace Training.Application.Sessions.UseCases
{
    public class UpdateSessionUseCase
    {
        private readonly ISessionRepository _sessionRepository;

        public UpdateSessionUseCase(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<SessionResponse?> ExecuteAsync(Guid id, UpdateSessionRequest request)
        {
            var session = await _sessionRepository.GetByIdAsync(id);

            if (session == null)
                return null;

            session.Update(
                request.DateDebut,
                request.DateFin,
                request.Capacite,
                request.Prix,
                request.AbonnementRequis,
                request.CoachId
            );

            await _sessionRepository.UpdateAsync(session);

            return SessionMapper.ToResponse(session);
        }
    }
}