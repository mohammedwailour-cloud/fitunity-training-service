using Training.Application.Coachs.Interfaces;
using Training.Application.Common.Interfaces;
using Training.Application.Events.Interfaces;
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
        private readonly IEventRepository _eventRepository;
        private readonly ISpaceRepository _spaceRepository;
        private readonly IUserContext _userContext;

        public UpdateSessionUseCase(
            ISessionRepository sessionRepository,
            ICoachRepository coachRepository,
            IEventRepository eventRepository,
            ISpaceRepository spaceRepository,
            IUserContext userContext)
        {
            _sessionRepository = sessionRepository;
            _coachRepository = coachRepository;
            _eventRepository = eventRepository;
            _spaceRepository = spaceRepository;
            _userContext = userContext;
        }

        public async Task<SessionResponse?> ExecuteAsync(Guid id, UpdateSessionRequest request)
        {
            if (request.DateDebut < DateTime.UtcNow)
                throw new InvalidSessionDatesException();

            Session? session = await _sessionRepository.GetByIdAsync(id);

            if (session == null)
                throw new SessionNotFoundException(id);

            if (_userContext.Role == "Coach" && session.CoachId != _userContext.UserId)
                throw new ForbiddenException("Coach can only modify their own sessions");

            if (request.IsOpenSession)
            {
                if (request.CoachId != null)
                    throw new InvalidOpenSessionException();

                if (request.ActivityId == null)
                    throw new InvalidOpenSessionException();
            }

            Space? space = await _spaceRepository.GetByIdAsync(request.SpaceId);

            if (space == null)
                throw new SpaceNotFoundException(request.SpaceId);

            if (!space.IsActive)
                throw new SpaceInactiveException();

            if (session.EventId.HasValue)
            {
                Event? ev = await _eventRepository.GetByIdAsync(session.EventId.Value);

                if (ev != null && ev.SpaceId != request.SpaceId)
                    throw new InvalidEventSpaceException();
            }

            if (space.Capacity.HasValue && !request.Capacite.HasValue)
                throw new InvalidSessionCapacityException();

            if (space.Capacity.HasValue && request.Capacite.HasValue && request.Capacite.Value > space.Capacity.Value)
                throw new InvalidSessionCapacityException();

            bool available = await _spaceRepository.IsSpaceAvailableAsync(
                request.SpaceId,
                request.DateDebut,
                request.DateFin,
                excludedSessionId: session.Id,
                excludedEventId: null);

            if (!available)
                throw new SpaceUnavailableException();

            if (!request.IsOpenSession)
            {
                await EnsureCoachMatchesActivityAsync(request.CoachId, request.ActivityId);
            }

            session.Update(
                session.Type,
                request.DateDebut,
                request.DateFin,
                request.Capacite,
                request.Prix,
                request.AbonnementRequis,
                request.SpaceId,
                request.ActivityId,
                request.CoachId,
                request.IsOpenSession
            );

            await _sessionRepository.UpdateAsync(session);

            Session? updatedSession = await _sessionRepository.GetByIdAsync(session.Id);

            if (updatedSession == null)
            {
                return SessionMapper.ToResponse(session, space);
            }

            return SessionMapper.ToResponse(updatedSession, updatedSession.Space ?? space);
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
