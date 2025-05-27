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
    public class GetUserRolesByIdQueryHandler : IRequestHandler<GetUserRolesByIdByUserIDQuery, GetUserRoleDto>
    {
        private readonly IUserRoleRepositoryMongo _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetUserRolesByIdQueryHandler(IUserRoleRepositoryMongo userRoleRepository, IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
        }

        public async Task<GetUserRoleDto> Handle(GetUserRolesByIdByUserIDQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userRoles = await _userRoleRepository.GetUserRolesByIdQuery(request.UserRoleId);

                if (userRoles == null  )
                {
                    Console.WriteLine($"No se encontraron roles para el usuario con ID: {request.UserRoleId}");
                    return new GetUserRoleDto(); // Retornar lista vacía en lugar de `null`
                }

                var userRolesDto = _mapper.Map<GetUserRoleDto>(userRoles);
                return userRolesDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return new  GetUserRoleDto(); // Retornar lista vacía en caso de error
            }
        }
    }
}
