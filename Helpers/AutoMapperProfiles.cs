using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DatingApp.API.DTOs;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // CreateMap<User, UserForListDTO>();
            // CreateMap<User, UserForDetailedDTO>();
            CreateMap<Photo, PhotoForUserDTO>();

            CreateMap<User, UserForDetailedDTO>()
                .ForMember(dest => dest.Age, opt => 
                    opt.MapFrom(src => (int)Math.Truncate((DateTime.Now - src.BirthDate).TotalDays / 365)))
                .ForMember(dest => dest.PhotoUrl, opt => 
                    opt.MapFrom(src => src.Photos.FirstOrDefault(o => o.isMainPhoto == true).Url));

            CreateMap<User, UserForListDTO>()
                .ForMember(dest => dest.Age, opt =>
                    opt.MapFrom(src => (int)Math.Truncate((DateTime.Now - src.BirthDate).TotalDays / 365)))
                .ForMember(dest => dest.PhotoUrl, opt =>
                    opt.MapFrom(src => src.Photos.FirstOrDefault(o => o.isMainPhoto == true).Url));
        }
    }
}