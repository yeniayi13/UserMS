using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;

namespace UserMs.Commoon.AutoMapper.ActivityHistory
{
    public class ActivityHistoryProfile : Profile
    {
        public ActivityHistoryProfile()
        {
            CreateMap<Domain.Entities.ActivityHistory.ActivityHistory, GetActivityHistoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                .ReverseMap(); // Permite convertir de DTO a entidad si es necesario
        }
    }
}
