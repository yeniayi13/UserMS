using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.Support;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Supports;
using UserMs.Core.Repositories.SupportsRepo;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Support.ValueObjects;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Support.Queries
{
    public class GetSupportByUserEmailQueryHandler : IRequestHandler<GetSupportByUserEmailQuery, GetSupportDto>
    {
        private readonly ISupportRepositoryMongo _supportRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        public GetSupportByUserEmailQueryHandler(ISupportRepositoryMongo supportRepository, IMapper mapper, IEventBus<GetActivityHistoryDto> eventBusActivity)
        {
            _supportRepository = supportRepository;
            _mapper = mapper;
            _eventBusActivity = eventBusActivity;
        }

        public async Task<GetSupportDto> Handle(GetSupportByUserEmailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var email = UserEmail.Create(request.Email);
                var support = await _supportRepository.GetSupportByEmailAsync(email);

                if (support == null)
                {
                   throw new UserNotFoundException($"No se encontró soporte con el email: {request.Email}");
                }
                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    support.UserId.Value,
                    "Busco un trabajador de soporte por email ",
                    DateTime.UtcNow
                );
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
                return _mapper.Map<GetSupportDto>(support);
            }
            catch (Exception ex)
            {
                throw;
               
            }
        }

        
    }
}
