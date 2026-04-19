using Training.Application.Common.Interfaces;
using Training.Application.Coachs.Interfaces;
using Training.Application.Events.Interfaces;
using Training.Application.Exceptions;
using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;
using Training.Application.Spaces.Interfaces;
using Training.Domain.Entities;
using Training.Domain.Events;
using Training.Domain.Exceptions;

namespace Training.Application.Sessions.UseCases
{
    public class CreateSessionUseCase
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ISpaceRepository _spaceRepository;
        private readonly IEventPublisher _eventPublisher;

        public CreateSessionUseCase(
            ISessionRepository sessionRepository,
            ICoachRepository coachRepository,
            IEventRepository eventRepository,
            ISpaceRepository spaceRepository,
            IEventPublisher eventPublisher)
        {
            _sessionRepository = sessionRepository;
            _coachRepository = coachRepository;
            _eventRepository = eventRepository;
            _spaceRepository = spaceRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<SessionResponse> Execute(CreateSessionRequest request)
        {
            if (request.DateDebut < DateTime.UtcNow)
                throw new InvalidSessionDatesException();

            Space? space = await _spaceRepository.GetByIdAsync(request.SpaceId);

            if (space == null)
                throw new SpaceNotFoundException(request.SpaceId);

            if (!space.IsActive)
                throw new SpaceInactiveException();

            if (request.EventId.HasValue)
            {
                Event? ev = await _eventRepository.GetByIdAsync(request.EventId.Value);

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
                request.DateFin);

            if (!available)
                throw new SpaceUnavailableException();

            await EnsureCoachMatchesActivityAsync(request.CoachId, request.ActivityId);

            Session session = SessionMapper.ToEntity(request);

            await _sessionRepository.AddAsync(session);

            SessionCreatedEvent domainEvent = new(session.Id);
            await _eventPublisher.PublishAsync(domainEvent);

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
