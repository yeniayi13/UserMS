using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.User_Roles;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.UserRoleRepo;

namespace UserMs.Application.Handlers.User_Roles.Queries___Copia
{
    public class GetUserRolesByRoleNameQueryHandler : IRequestHandler<GetUserRolesByRoleNameQuery, List<GetUserRoleDto>>
    {
        private readonly IUserRoleRepositoryMongo _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetUserRolesByRoleNameQueryHandler(IUserRoleRepositoryMongo userRoleRepository, IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
        }

        public async Task<List<GetUserRoleDto>> Handle(GetUserRolesByRoleNameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userRoles = await _userRoleRepository.GetUserRolesByRoleNameQuery(request.RoleName);

                if (userRoles == null)
                {
                    Console.WriteLine($"No se encontraron usuario con el rol de : {request.RoleName}");
                    return new List<GetUserRoleDto>(); // Retornar lista vacía en lugar de `null`
                }

                var userRolesDto = _mapper.Map<List<GetUserRoleDto>>(userRoles);
                return userRolesDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return new List<GetUserRoleDto>(); // Retornar lista vacía en caso de error
            }
        }
    }
}
