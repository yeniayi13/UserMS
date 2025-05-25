using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Domain.Entities.Role;

namespace UserMs.Commoon.AutoMapper.Role
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<Roles, GetRoleDto>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId.Value))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName.Value))
                //.ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted.Value))
                .ReverseMap();
        }
    }
}
