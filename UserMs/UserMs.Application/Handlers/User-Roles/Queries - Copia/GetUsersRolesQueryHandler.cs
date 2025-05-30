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

namespace UserMs.Application.Handlers.User_Roles.Queries
{
    public class GetUsersRolesQueryHandler : IRequestHandler<GetUsersRolesQuery, List<GetUserRoleDto>>
    {
        private readonly IUserRoleRepositoryMongo _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetUsersRolesQueryHandler(IUserRoleRepositoryMongo userRoleRepository, IMapper mapper)
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
                throw; // Retornar lista vacía en caso de error
            }
        }
    }
}
