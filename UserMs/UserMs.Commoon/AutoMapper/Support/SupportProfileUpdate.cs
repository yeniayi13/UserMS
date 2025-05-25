using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Domain.Entities.Support;

namespace UserMs.Commoon.AutoMapper.Support
{
    public class SupportProfileUpdate : Profile
    {
        public SupportProfileUpdate()
        {
            CreateMap<Supports, GetSupportDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.Value))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.UserEmail.Value))
                //.ForMember(dest => dest.UserPassword, opt => opt.MapFrom(src => src.UserPassword.Value)) // ⚠️ Considera encriptar antes de almacenar
                .ForMember(dest => dest.UserAddress, opt => opt.MapFrom(src => src.UserAddress.Value))
                .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.UserPhone.Value))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName.Value))
                .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.UserLastName.Value))
                .ForMember(dest => dest.SupportDni, opt => opt.MapFrom(src => src.SupportDni != null ? src.SupportDni.Value : string.Empty))
                .ForMember(dest => dest.SupportSpecialization, opt => opt.MapFrom(src => src.SupportSpecialization != null ? src.SupportSpecialization.ToString() : string.Empty)) // Convertir Enum a string
                .ForMember(dest => dest.SupportDelete, opt => opt.MapFrom(src => src.SupportDelete.Value))
                .ReverseMap();
        }
    }
}
