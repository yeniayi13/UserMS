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
using UserMs.Domain.Entities.Bidder.ValueObjects;


namespace UserMs.Application.Handlers.Bidder.Queries
{
    public class GetBidderByIdQueryHandler : IRequestHandler<GetBidderByIdQuery, GetBidderDto>
    {
        private readonly IBidderRepositoryMongo _bidderRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        public GetBidderByIdQueryHandler(IEventBus<GetActivityHistoryDto> eventBusActivity,IActivityHistoryRepository activityHistoryRepository, IBidderRepositoryMongo bidderRepository, IMapper mapper)
        {
            _bidderRepository = bidderRepository;
            _mapper = mapper;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }

        public async Task<GetBidderDto> Handle(GetBidderByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var id = UserId.Create(request.BidderId);
                var bidder = await _bidderRepository.GetBidderByIdAsync(id);

                if (bidder == null)
                {
                    Console.WriteLine($"No se encontró postor con ID: {request.BidderId}");
                    return null;
                }
                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    request.BidderId,
                    "Busco Postores por Id",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
                return _mapper.Map<GetBidderDto>(bidder);
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return null;
            }
        }
    }
}
