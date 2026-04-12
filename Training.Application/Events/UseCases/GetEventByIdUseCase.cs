using System;
using System.Collections.Generic;
using System.Linq;
using Training.Application.Events.DTOs;
using Training.Application.Events.Interfaces;
using Training.Application.Events.Mappers;

namespace Training.Application.Events.UseCases
{
    public class GetEventByIdUseCase
    {
        private readonly IEventRepository _repository;

        public GetEventByIdUseCase(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<EventDto> Execute(Guid id)
        {
            var ev = await _repository.GetByIdAsync(id);

            if (ev == null)
                throw new Exception("Event not found");

            return EventMapper.ToDto(ev);
        }
    }
}
