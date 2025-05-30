using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.Bidder;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Biddere;
using UserMs.Core.Repositories.Bidders;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Bidder.Queries
{
    public class GetBidderByUserEmailQueryHandler : IRequestHandler<GetBidderByUserEmailQuery, GetBidderDto>
    {
        private readonly IBidderRepositoryMongo _bidderRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        public GetBidderByUserEmailQueryHandler(IEventBus<GetActivityHistoryDto> eventBusActivity,IActivityHistoryRepository activityHistoryRepository, IBidderRepositoryMongo bidderRepository, IMapper mapper)
        {
            _bidderRepository = bidderRepository;
            _mapper = mapper;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }

        public async Task<GetBidderDto> Handle(GetBidderByUserEmailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var email = UserEmail.Create(request.Email);
                var bidder = await _bidderRepository.GetBidderByEmailAsync(email);

                if (bidder == null)
                {
                    throw new UserNotFoundException($"No se encontró soporte con el email: {request.Email}");
                }
                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    bidder.UserId.Value,
                    "Busco Postores",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
                return _mapper.Map<GetBidderDto>(bidder);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
