using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Dtos.Users.Response;
using UserMs.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UserMs.Commoon.AutoMapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Users, GetUsersDto>().ReverseMap();
        }
    }
}
