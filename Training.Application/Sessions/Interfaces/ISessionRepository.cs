using Training.Domain.Entities;

namespace Training.Application.Sessions.Interfaces;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid id);

    Task<IEnumerable<Session>> GetAllAsync();

    Task<IEnumerable<Session>> GetByIdsAsync(IEnumerable<Guid> sessionIds);

    Task AddAsync(Session session);

    Task UpdateAsync(Session session);

    Task<(IEnumerable<Session>, int totalCount)> GetPagedAsync(int page, int pageSize);

    Task<bool> IsSpaceAvailableAsync(Guid spaceId, DateTime start, DateTime end, Guid? excludedSessionId = null);

    Task DeleteAsync(Guid id);
}
