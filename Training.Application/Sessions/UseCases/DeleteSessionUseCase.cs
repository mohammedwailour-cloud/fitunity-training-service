using Training.Application.Common.Interfaces;
using Training.Application.Exceptions;
using Training.Application.Sessions.Interfaces;
using Training.Domain.Entities;

namespace Training.Application.Sessions.UseCases
{
    public class DeleteSessionUseCase
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IUserContext _userContext;

        public DeleteSessionUseCase(ISessionRepository sessionRepository, IUserContext userContext)
        {
            _sessionRepository = sessionRepository;
            _userContext = userContext;
        }

        public async Task<bool> ExecuteAsync(Guid id)
        {
            Session? session = await _sessionRepository.GetByIdAsync(id);

            if (session == null)
                return false;

            if (_userContext.Role == "Coach" && session.CoachId != _userContext.UserId)
                throw new ForbiddenException("Coach can only modify their own sessions");

            await _sessionRepository.DeleteAsync(id);
            return true;
        }
    }
}
