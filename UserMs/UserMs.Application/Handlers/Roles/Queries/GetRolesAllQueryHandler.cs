using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.Roles;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.RolesRepo;

namespace UserMs.Application.Handlers.Roles.Queries
{
    public class GetRolesAllQueryHandler : IRequestHandler<GetRolesAllQuery, List<GetRoleDto>>
    {
        private readonly IRolesRepository _rolesRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetRolesAllQueryHandler(IRolesRepository rolesRepository, IMapper mapper)
        {
            _rolesRepository = rolesRepository;
            _mapper = mapper; // Inyectar el Mapper
        }

        public async Task<List<GetRoleDto>> Handle(GetRolesAllQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var roles = await _rolesRepository.GetRolesAllQueryAsync();

                if (roles == null || roles.Count == 0)
                {
                    Console.WriteLine("No se encontraron roles.");
                    return new List<GetRoleDto>(); // Retornar lista vacía en lugar de `null`
                }

                var rolesDto = _mapper.Map<List<GetRoleDto>>(roles);
                return rolesDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return new List<GetRoleDto>(); // Retornar lista vacía en caso de error
            }
        }
    }
}
