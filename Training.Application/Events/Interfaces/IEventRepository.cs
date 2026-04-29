using Training.Domain.Entities;

namespace Training.Application.Events.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<(int TotalCount, List<Event> Items)> GetAllAsync(int page, int pageSize);
    Task AddAsync(Event ev);
    Task UpdateAsync(Event ev);
    Task<bool> IsSpaceAvailableAsync(Guid spaceId, DateTime start, DateTime end, Guid? excludedEventId = null);
    Task DeleteAsync(Guid id);
}
