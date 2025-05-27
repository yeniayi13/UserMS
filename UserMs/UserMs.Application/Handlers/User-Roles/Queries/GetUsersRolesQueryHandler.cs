using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.User_Roles;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.Repositories.UserRoleRepo;

namespace UserMs.Application.Handlers.User_Roles.Queries
{
    public class GetUsersRolesQueryHandler : IRequestHandler<GetUsersRolesQuery, List<GetUserRoleDto>>
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IMapper _mapper;

        public GetUsersRolesQueryHandler(IUserRoleRepository userRoleRepository, IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
        }

        public async Task<List<GetUserRoleDto>> Handle(GetUsersRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userRoles = await _userRoleRepository.GetUsersRoleAsync();

                if (userRoles == null || userRoles.Count == 0)
                {
                    Console.WriteLine("No se encontraron roles de usuarios.");
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
