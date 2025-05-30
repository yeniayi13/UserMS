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
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.User_Roles.Queries___Copia
{
    public class GetRoleByRoleNameAndByUserEmailQueryHandler : IRequestHandler<GetRoleByIdAndByUserIdQuery, bool>
    {
        private readonly IUserRoleRepositoryMongo _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetRoleByRoleNameAndByUserEmailQueryHandler(IUserRoleRepositoryMongo userRoleRepository, IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
        }

        public async Task<bool> Handle(GetRoleByIdAndByUserIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userRoles = await _userRoleRepository.GetRoleByRoleNameAndByUserEmail(request.RoleId,request.UserId);

                if (userRoles == null)
                {
                    throw new RoleNotFoundException(); // Retornar lista vacía en lugar de `null`
                }

               // var userRolesDto = _mapper.Map<GetUserRoleDto>(userRoles);
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
