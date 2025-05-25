using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.User_Roles;

namespace UserMs.Commoon.AutoMapper.UserRole
{
    public class UserRolesProfile : Profile
    {
        public UserRolesProfile()
        {
            CreateMap<UserRoles, GetUserRoleDto>()
                .ForMember(dest => dest.UserRoleId, opt => opt.MapFrom(src => src.UserRoleId.Value))
                //.ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src))
                //.ForMember(dest => dest.User.UserName, opt => opt.MapFrom(src => src.UserName.Value))
                
                .ReverseMap();
        }
    }
}
