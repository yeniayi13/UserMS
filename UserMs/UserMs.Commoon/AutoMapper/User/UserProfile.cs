using AutoMapper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.UserEntity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UserMs.Commoon.AutoMapper.User
{
    [ExcludeFromCodeCoverage]
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Users, GetUsersDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.Value))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.UserEmail.Value))
                .ForMember(dest => dest.UserPassword, opt => opt.MapFrom(src => src.UserPassword.Value)) // ⚠️ Revisa si es seguro mapear contraseñas
                .ForMember(dest => dest.UserDelete, opt => opt.MapFrom(src => src.UserDelete.Value )) // Manejo seguro de valores nulos
                .ForMember(dest => dest.UserAddress, opt => opt.MapFrom(src => src.UserAddress.Value))
                .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.UserPhone.Value ))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName.Value))
                .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.UserLastName.Value))
                .ForMember(dest => dest.UsersType, opt => opt.MapFrom(src => src.UsersType.ToString())) // Convertir Enum a string
                .ForMember(dest => dest.UserAvailable, opt => opt.MapFrom(src => src.UserAvailable.ToString())) // Convertir Enum a string
                .ReverseMap();
        }
    }
}
