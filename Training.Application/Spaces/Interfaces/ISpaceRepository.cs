using Training.Domain.Entities;

namespace Training.Application.Spaces.Interfaces;

public interface ISpaceRepository
{
    Task<Space> AddAsync(Space space);
    Task<Space?> GetByIdAsync(Guid id);
    Task UpdateAsync(Space space);
    Task DeleteAsync(Guid id);
    Task<(IEnumerable<Space> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludedSpaceId = null);
}
