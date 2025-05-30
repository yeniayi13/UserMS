using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using UserMs.Infrastructure.Repositories.ActivityHistoryRepo;

namespace UserMs.Application.Handlers.Auctioneer.Queries
{
    public class GetAuctioneerByIdQueryHandler : IRequestHandler<GetAuctioneerByIdQuery, GetAuctioneerDto>
    {
        private readonly IAuctioneerRepositoryMongo _auctioneerRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;

        public GetAuctioneerByIdQueryHandler(IEventBus<GetActivityHistoryDto> eventBusActivity,IActivityHistoryRepository activityHistoryRepository, IAuctioneerRepositoryMongo auctioneerRepository, IMapper mapper)
        {
            _auctioneerRepository = auctioneerRepository;
            _mapper = mapper;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }

        public async Task<GetAuctioneerDto> Handle(GetAuctioneerByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var id = UserId.Create(request.AuctioneerId);
                var auctioneer = await _auctioneerRepository.GetAuctioneerByIdAsync(id);

                if (auctioneer == null)
                {
                    throw new UserNotFoundException($"No se encontró soporte con el email: {request.AuctioneerId}");
                }
                // **Registrar actividad en `ActivityHistoryRepository`**
                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    id.Value,
                    "Busco Subastador por Id ",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
                return _mapper.Map<GetAuctioneerDto>(auctioneer);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
