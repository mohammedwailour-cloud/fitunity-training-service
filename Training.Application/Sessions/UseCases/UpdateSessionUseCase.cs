using Training.Application.Coachs.Interfaces;
using Training.Application.Exceptions;
using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;
using Training.Application.Spaces.Interfaces;
using Training.Domain.Entities;
using Training.Domain.Exceptions;

namespace Training.Application.Sessions.UseCases
{
    public class UpdateSessionUseCase
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly ISpaceRepository _spaceRepository;

        public UpdateSessionUseCase(
            ISessionRepository sessionRepository,
            ICoachRepository coachRepository,
            ISpaceRepository spaceRepository)
        {
            _sessionRepository = sessionRepository;
            _coachRepository = coachRepository;
            _spaceRepository = spaceRepository;
        }

        public async Task<SessionResponse?> ExecuteAsync(Guid id, UpdateSessionRequest request)
        {
            if (request.DateDebut < DateTime.UtcNow)
                throw new InvalidSessionDatesException();

            Session? session = await _sessionRepository.GetByIdAsync(id);

            if (session == null)
                throw new SessionNotFoundException(id);

            Space? space = await _spaceRepository.GetByIdAsync(request.SpaceId);

            if (space == null)
                throw new SpaceNotFoundException(request.SpaceId);

            if (!space.IsActive)
                throw new SpaceInactiveException();

            if (space.Capacity.HasValue && !request.Capacite.HasValue)
                throw new InvalidSessionCapacityException();

            if (space.Capacity.HasValue && request.Capacite.HasValue && request.Capacite.Value > space.Capacity.Value)
                throw new InvalidSessionCapacityException();

            bool available = await _sessionRepository.IsSpaceAvailableAsync(
                request.SpaceId,
                request.DateDebut,
                request.DateFin,
                session.Id);

            if (!available)
                throw new SpaceUnavailableException();

            await EnsureCoachMatchesActivityAsync(request.CoachId, session.ActivityId);

            session.Update(
                request.DateDebut,
                request.DateFin,
                request.Capacite,
                request.Prix,
                request.AbonnementRequis,
                request.SpaceId,
                request.CoachId
            );

            await _sessionRepository.UpdateAsync(session);

            return SessionMapper.ToResponse(session, space);
        }

        private async Task EnsureCoachMatchesActivityAsync(Guid? coachId, Guid? activityId)
        {
            if (!coachId.HasValue)
                return;

            Coach? coach = await _coachRepository.GetByIdAsync(coachId.Value);

            if (coach == null)
                throw new CoachNotFoundException(coachId.Value);

            if (!activityId.HasValue || coach.ActivityId != activityId.Value)
                throw new CoachActivityMismatchException(coach.Id, activityId ?? Guid.Empty);
        }
    }
}
