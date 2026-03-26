using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Training.Domain.Entities;

namespace Training.Application.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id);

    Task AddAsync(Reservation reservation);

    Task UpdateAsync(Reservation reservation);

    Task<IEnumerable<Reservation>> GetBySessionIdAsync(Guid sessionId);

    Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId);

    Task<(IEnumerable<Reservation>, int totalCount)> GetPagedAsync(int page, int pageSize);
}

