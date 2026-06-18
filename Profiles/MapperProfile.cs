using AutoMapper;
using OnlineJobRecruitmentSystem.DTOs;
using OnlineJobRecruitmentSystem.DTOs.UserDtos.OnlineJobRecruitmentSystem.DTOs.UserDtos;
using OnlineJobRecruitmentSystem.Models;

namespace OnlineJobRecruitmentSystem.Profiles
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
