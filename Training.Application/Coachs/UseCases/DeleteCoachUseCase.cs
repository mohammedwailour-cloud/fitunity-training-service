using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Training.Application.Coachs.Interfaces;

namespace Training.Application.Coachs.UseCases
{
    public class DeleteCoachUseCase
    {
        private readonly ICoachRepository _repository;

        public DeleteCoachUseCase(ICoachRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
