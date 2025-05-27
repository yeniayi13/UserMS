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
using UserMs.Core.Repositories.Bidders;

namespace UserMs.Application.Handlers.Bidder.Queries
{
    public class GetBidderAllQueryHandler : IRequestHandler<GetBidderAllQuery, List<GetBidderDto>>
    {
        private readonly IBidderRepositoryMongo _bidderRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        public GetBidderAllQueryHandler(IEventBus<GetActivityHistoryDto> eventBusActivity,IActivityHistoryRepository activityHistoryRepository, IBidderRepositoryMongo bidderRepository, IMapper mapper)
        {
            _bidderRepository = bidderRepository;
            _mapper = mapper; // Inyectar el Mapper
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }

        public async Task<List<GetBidderDto>> Handle(GetBidderAllQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var bidders = await _bidderRepository.GetBidderAllAsync();

                if (bidders == null || bidders.Count == 0)
                {
                    Console.WriteLine("No se encontraron postores.");
                    return new List<GetBidderDto>(); // Retornar lista vacía en lugar de `null`
                }
              
                return _mapper.Map<List<GetBidderDto>>(bidders);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return new List<GetBidderDto>(); // Retornar lista vacía en caso de error
            }
        }
    }
}
