using AutoMapper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Domain.Entities.Auctioneer;

namespace UserMs.Commoon.AutoMapper.Auctioneer
{
    [ExcludeFromCodeCoverage]
    public class AuctioneerProfileUpdate : Profile
    {
        public AuctioneerProfileUpdate()
        {
            CreateMap<Auctioneers, UpdateAuctioneerDto>()
                //.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.Value))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.UserEmail.Value))
                //.ForMember(dest => dest.UserPassword, opt => opt.MapFrom(src => src.UserPassword.Value)) // ⚠️ Considera encriptar antes de almacenar
                .ForMember(dest => dest.UserAddress, opt => opt.MapFrom(src => src.UserAddress.Value))
                .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.UserPhone.Value))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName.Value))
                .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.UserLastName.Value))
                .ForMember(dest => dest.AuctioneerDni, opt => opt.MapFrom(src => src.AuctioneerDni.Value))
                .ForMember(dest => dest.AuctioneerBirthday, opt => opt.MapFrom(src => src.AuctioneerBirthday.Value))
                //.ForMember(dest => dest.AuctioneerDelete, opt => opt.MapFrom(src => src.AuctioneerDelete.Value))
                .ReverseMap();
        }
    }
}
