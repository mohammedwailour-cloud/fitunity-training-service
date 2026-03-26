using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Entities;

namespace Training.Application.UseCases
{
    public class GetSessionsPagedUseCase
    {
        private readonly ISessionRepository _sessionRepository;

        public GetSessionsPagedUseCase(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<PagedResult<Session>> ExecuteAsync(int page, int pageSize)
        {
            var (sessions, totalCount) =
                await _sessionRepository.GetPagedAsync(page, pageSize);

            return new PagedResult<Session>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Data = sessions
            };
        }
    }
}
