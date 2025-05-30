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
    public class GetAuctioneerByUserEmailQueryHandler : IRequestHandler<GetAuctioneerByUserEmailQuery, GetAuctioneerDto>
    {
        private readonly IAuctioneerRepositoryMongo _auctioneerRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        public GetAuctioneerByUserEmailQueryHandler(IEventBus<GetActivityHistoryDto> eventBusActivity,IActivityHistoryRepository activityHistoryRepository, IAuctioneerRepositoryMongo auctioneerRepository, IMapper mapper)
        {
            _auctioneerRepository = auctioneerRepository;
            _mapper = mapper;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }

        public async Task<GetAuctioneerDto> Handle(GetAuctioneerByUserEmailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var email = UserEmail.Create(request.Email);
                var auctioneer = await _auctioneerRepository.GetAuctioneerByEmailAsync(email);

                if (auctioneer == null)
                {
                    throw new UserNotFoundException($"No se encontró soporte con el email: {request.Email}");
                }
                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    auctioneer.UserId.Value,
                    "Busco Subastador por email ",
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
