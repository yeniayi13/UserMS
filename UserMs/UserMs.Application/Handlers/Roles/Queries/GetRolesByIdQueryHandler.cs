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
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Roles.Queries
{
    public class GetRolesByIdQueryHandler : IRequestHandler<GetRolesByIdQuery, GetRoleDto>
    {
        private readonly IRolesRepository _rolesRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetRolesByIdQueryHandler(IRolesRepository rolesRepository, IMapper mapper)
        {
            _rolesRepository = rolesRepository;
            _mapper = mapper;
        }

        public async Task<GetRoleDto> Handle(GetRolesByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _rolesRepository.GetRolesByIdQuery(request.RoleId);
                if (role == null)
                    throw new RoleNotFoundException("Role not found.");

                var roleDto = _mapper.Map<GetRoleDto>(role);
                return roleDto;
            }
            catch (Exception ex)
            {
                throw; // Retornar lista vacía en caso de error
            }
        }
    }
}
