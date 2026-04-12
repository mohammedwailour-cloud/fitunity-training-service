using Training.Domain.Entities;

namespace Training.Application.Events.Interfaces;

public interface IEventRepository
{
    Task<Event> GetByIdAsync(Guid id);
    Task<List<Event>> GetAllAsync(int page, int pageSize);
    Task AddAsync(Event ev);
    Task UpdateAsync(Event ev);
    Task DeleteAsync(Guid id);
}