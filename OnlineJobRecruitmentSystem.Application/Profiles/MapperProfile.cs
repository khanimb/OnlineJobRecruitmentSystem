using AutoMapper;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Application.Profiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RegisterDto, User>();
            CreateMap<User, UserResponseDto>();
        }
    }
}
