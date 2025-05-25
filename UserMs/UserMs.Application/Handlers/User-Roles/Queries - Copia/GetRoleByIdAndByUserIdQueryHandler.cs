using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.Roles;
using UserMs.Application.Queries.User_Roles;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.UserRoleRepo;

namespace UserMs.Application.Handlers.User_Roles.Queries___Copia
{
    internal class GetRoleByIdAndByUserIdQueryHandler : IRequestHandler<GetRoleByIdAndByUserIdQuery, bool>
    {
        private readonly IUserRoleRepositoryMongo _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetRoleByIdAndByUserIdQueryHandler(IUserRoleRepositoryMongo userRoleRepository, IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
        }

        public async Task<bool> Handle(GetRoleByIdAndByUserIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userRoles = await _userRoleRepository.GetRoleByIdAndByUserIdQuery(request.RoleId,request.UserId);

                if (userRoles == null)
                {
                    Console.WriteLine($"No se encontraron roles para el usuario con ID: {request.UserId}");
                    return false; // Retornar lista vacía en lugar de `null`
                }

               // var userRolesDto = _mapper.Map<GetUserRoleDto>(userRoles);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return false; // Retornar lista vacía en caso de error
            }
        }
    }
}
