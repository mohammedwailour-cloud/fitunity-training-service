using Training.Domain.Entities;

namespace Training.Application.Coachs.Interfaces
{
    public interface ICoachRepository
    {
        Task<Coach> GetByIdAsync(Guid id);
        Task<List<Coach>> GetAllAsync(int page, int pageSize);
        Task AddAsync(Coach coach);
        Task UpdateAsync(Coach coach);
        Task DeleteAsync(Guid id);
    }
}
