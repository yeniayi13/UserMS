using AutoMapper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Domain.Entities.Role_Permission;

namespace UserMs.Commoon.AutoMapper.Role_Permission
{
    [ExcludeFromCodeCoverage]
    public class RolePermissionProfile : Profile
    {
        public RolePermissionProfile()
        {
            CreateMap<RolePermissions, GetRolePermissionDto>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId.Value))
                .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.PermissionId.Value))
                //.ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.PermissionName.Value))
                //.ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName.Value))
                   
                .ReverseMap();
        }
    }
}
