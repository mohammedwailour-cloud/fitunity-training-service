using Training.Application.Common.DTOs;
using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;
using Training.Domain.Entities;

namespace Training.Application.Sessions.UseCases
{
    public class GetSessionsPagedUseCase
    {
        private readonly ISessionRepository _sessionRepository;

        public GetSessionsPagedUseCase(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<PagedResult<SessionResponse>> ExecuteAsync(int page, int pageSize)
        {
            (IEnumerable<Session> sessions, int totalCount) = await _sessionRepository.GetPagedAsync(page, pageSize);
            List<SessionResponse> responses = sessions
                .Select(session => SessionMapper.ToResponse(session, session.Space))
                .ToList();

            return new PagedResult<SessionResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Data = responses
            };
        }
    }
}
