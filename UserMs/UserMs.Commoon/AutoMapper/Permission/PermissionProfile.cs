using AutoMapper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Permission;
using UserMs.Domain.Entities.Permission;

namespace UserMs.Commoon.AutoMapper.Permission
{
    [ExcludeFromCodeCoverage]
    public class PermissionProfile : Profile
    {
        public PermissionProfile()
        {
            CreateMap<Permissions, GetPermissionDto>()
                .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.PermissionId.Value))
                .ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.PermissionName.Value))
                .ReverseMap();
        }
    }

}
