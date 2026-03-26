using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Training.Domain.Entities;

namespace Training.Application.Interfaces;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid id);

    Task<IEnumerable<Session>> GetAllAsync();

    Task AddAsync(Session session);

    Task UpdateAsync(Session session);

    Task<(IEnumerable<Session>, int totalCount)> GetPagedAsync(int page, int pageSize);

    
}

