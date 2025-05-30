using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.Permission;
using UserMs.Application.Queries.Roles;
using UserMs.Commoon.Dtos.Users.Response.Permission;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.PermissionRepo;
using UserMs.Core.Repositories.RolesRepo;

namespace UserMs.Application.Handlers.Permission
{
    public class GetPermissionAllQueryHandler : IRequestHandler<GetPermissionAllQuery, List<GetPermissionDto>>
    {
        private readonly IPermissionRepositoryMongo _permissionRepository;
        private readonly IMapper _mapper;

        public GetPermissionAllQueryHandler(IPermissionRepositoryMongo permissionRepository, IMapper mapper)
        {
            _permissionRepository = permissionRepository;
            _mapper = mapper; // Inyectar el Mapper
        }

        public async Task<List<GetPermissionDto>> Handle(GetPermissionAllQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var permision = await _permissionRepository.GetPermissionAllQueryAsync();

                if (permision == null || permision.Count == 0)
                {
                    Console.WriteLine("No se encontraron roles.");
                    return new List<GetPermissionDto>(); // Retornar lista vacía en lugar de `null`
                }

                var permisisonDto = _mapper.Map<List<GetPermissionDto>>(permision);

                return permisisonDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return new List<GetPermissionDto>(); // Retornar lista vacía en caso de error
            }
        }
    }
}
