using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Domain.Entities;

namespace UserMs.Application.Handlers.ActivityHistory.NewFolder
{
    public class CreateHistoryActivityCommandHandler : IRequestHandler<CreateHistoryActivityCommand, UserId>
    {
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IMapper _mapper;
        private readonly IEventBus<GetActivityHistoryDto> _eventBus;

        public CreateHistoryActivityCommandHandler(
            IActivityHistoryRepository activityHistoryRepository,
            IMapper mapper,
            IEventBus<GetActivityHistoryDto> eventBus)
        {
            _activityHistoryRepository = activityHistoryRepository;
            _mapper = mapper;
            _eventBus = eventBus;
        }

        public async Task<UserId> Handle(CreateHistoryActivityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"Registrando actividad para el usuario ID: {request.HistoryActivity.UserId}");

                var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    request.HistoryActivity.UserId,
                    request.HistoryActivity.Action,
                    DateTime.UtcNow
                );

                await _activityHistoryRepository.AddAsync(activityHistory);

                var activityDto = _mapper.Map<GetActivityHistoryDto>(activityHistory);
                await _eventBus.PublishMessageAsync(activityDto, "activityQueue", "ACTIVITY_CREATED");

                return activityHistory.UserId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar actividad: {ex.Message}");
                throw;
            }
        }
    }
}
