using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Auctioneer;

namespace UserMs.Application.Handlers.Auctioneer.Queries
{
    public class GetAuctioneerAllQueryHandler : IRequestHandler<GetAuctioneerAllQuery, List<GetAuctioneerDto>>
    {
        private readonly IAuctioneerRepositoryMongo _auctioneerRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetAuctioneerAllQueryHandler(IAuctioneerRepositoryMongo auctioneerRepository, IMapper mapper)
        {
            _auctioneerRepository = auctioneerRepository;
            _mapper = mapper; // Inyectar el Mapper
        }

        public async Task<List<GetAuctioneerDto>> Handle(GetAuctioneerAllQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var auctioneers = await _auctioneerRepository.GetAuctioneerAllAsync();

                if (auctioneers == null || auctioneers.Count == 0)
                {
                    Console.WriteLine("No se encontraron subastadores.");
                    return new List<GetAuctioneerDto>(); // Retornar lista vacía en lugar de `null`
                }
                

                return _mapper.Map<List<GetAuctioneerDto>>(auctioneers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return new List<GetAuctioneerDto>(); // Retornar lista vacía en caso de error
            }
        }
    }
}
