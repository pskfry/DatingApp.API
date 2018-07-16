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
            CreateMap<Photo, PhotoForUserDTO>();
            CreateMap<UserForUpdateDTO, User>();
            
            CreateMap<PhotoForCreationDTO, Photo>();
            CreateMap<Photo, PhotoForReturnDTO>();

            CreateMap<MessageForCreationDTO, Message>();
            CreateMap<Message, MessageForReturnDTO>()
                .ForMember(dest => dest.SenderPhotoUrl, opt =>
                    opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.isMainPhoto).Url))
                .ForMember(dest => dest.RecipientPhotoUrl, opt =>
                    opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p => p.isMainPhoto).Url));

            CreateMap<UserForRegisterDTO, User>();

            CreateMap<User, UserForDetailedDTO>()
                .ForMember(dest => dest.Age, opt => 
                    opt.MapFrom(src => Extensions.CalculateAge(src.BirthDate)))
                .ForMember(dest => dest.PhotoUrl, opt => 
                    opt.MapFrom(src => src.Photos.FirstOrDefault(o => o.isMainPhoto == true).Url));

            CreateMap<User, UserForListDTO>()
                .ForMember(dest => dest.Age, opt =>
                    opt.MapFrom(src => Extensions.CalculateAge(src.BirthDate)))
                .ForMember(dest => dest.PhotoUrl, opt =>
                    opt.MapFrom(src => src.Photos.FirstOrDefault(o => o.isMainPhoto == true).Url));
        }
    }
}