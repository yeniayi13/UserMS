using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.Support;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Supports;
using UserMs.Core.Repositories.SupportsRepo;

namespace UserMs.Application.Handlers.Support.Queries
{
    public class GetSupportAllQueryHandler : IRequestHandler<GetSupportAllQuery, List<GetSupportDto>>
    {
        private readonly ISupportRepositoryMongo _supportRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetSupportAllQueryHandler(ISupportRepositoryMongo supportRepository, IMapper mapper)
        {
            _supportRepository = supportRepository;
            _mapper = mapper; // Inyectar el Mapper
        }

        public async Task<List<GetSupportDto>> Handle(GetSupportAllQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var supports = await _supportRepository.GetSupportAllAsync();

                if (supports == null || supports.Count == 0)
                {
                    Console.WriteLine("No se encontraron registros de soporte.");
                    return new List<GetSupportDto>(); // Retornar lista vacía en lugar de `null`
                }

                return _mapper.Map<List<GetSupportDto>>(supports);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return new List<GetSupportDto>(); // Retornar lista vacía en caso de error
            }
        }
    }
}
