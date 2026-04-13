using Training.Application.Coachs.Interfaces;
using Training.Application.Exceptions;
using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;
using Training.Domain.Exceptions;

namespace Training.Application.Sessions.UseCases
{
    public class UpdateSessionUseCase
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly ICoachRepository _coachRepository;

        public UpdateSessionUseCase(ISessionRepository sessionRepository, ICoachRepository coachRepository)
        {
            _sessionRepository = sessionRepository;
            _coachRepository = coachRepository;
        }

        public async Task<SessionResponse?> ExecuteAsync(Guid id, UpdateSessionRequest request)
        {
            var session = await _sessionRepository.GetByIdAsync(id);

            if (session == null)
                throw new SessionNotFoundException(id);

            await EnsureCoachMatchesActivityAsync(request.CoachId, session.ActivityId);

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

        private async Task EnsureCoachMatchesActivityAsync(Guid? coachId, Guid? activityId)
        {
            if (!coachId.HasValue)
                return;

            var coach = await _coachRepository.GetByIdAsync(coachId.Value);

            if (coach == null)
                throw new CoachNotFoundException(coachId.Value);

            if (!activityId.HasValue || coach.ActivityId != activityId.Value)
                throw new CoachActivityMismatchException(coach.Id, activityId ?? Guid.Empty);
        }
    }
}
