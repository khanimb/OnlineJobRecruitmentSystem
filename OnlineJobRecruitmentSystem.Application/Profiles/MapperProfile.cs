using AutoMapper;
using OnlineJobRecruitmentSystem.Application.DTOs.AnalyticsDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.EmployerDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.JobAlertDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.JobDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.JobSeekerDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.JobApplicationDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.PaymentDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.PortfolioDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Application.Profiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<User, UserResponseDto>();

            CreateMap<Message, ReturnMessageDto>()
                .ForMember(d => d.SenderName, o => o.MapFrom(s => s.Sender.Email))
                .ForMember(d => d.ReceiverName, o => o.MapFrom(s => s.Receiver.Email));

            CreateMap<Comment, ReturnCommentDto>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.User.Username));

            CreateMap<CreateNotificationDto, Notification>();
            CreateMap<Notification, ReturnNotificationDto>();

            CreateMap<CreateReviewDto, Review>();
            CreateMap<UpdateReviewDto, Review>();
            CreateMap<Review, ReturnReviewDto>()
                .ForMember(d => d.ReviewerName, o => o.MapFrom(s => s.Reviewer.Email))
                .ForMember(d => d.RevieweeName, o => o.MapFrom(s => s.Reviewee.Email));

            CreateMap<CreateJobAlertDto, JobAlert>();
            CreateMap<UpdateJobAlertDto, JobAlert>();
            CreateMap<JobAlert, ReturnJobAlertDto>();

            CreateMap<CreatePortfolioItemDto, PortfolioItem>()
                .ForMember(d => d.FileUrl, o => o.Ignore())
                .ForMember(d => d.FileType, o => o.Ignore());
            CreateMap<UpdatePortfolioItemDto, PortfolioItem>()
                .ForMember(d => d.FileUrl, o => o.Ignore())
                .ForMember(d => d.FileType, o => o.Ignore());
            CreateMap<PortfolioItem, ReturnPortfolioItemDto>();

            CreateMap<CreatePaymentDto, PaymentPremium>()
                .ForMember(d => d.StripePaymentId, o => o.Ignore())
                .ForMember(d => d.Amount, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore());
            CreateMap<PaymentPremium, ReturnPaymentDto>();

            CreateMap<JobApplication, ApplicationTrendDto>()
                .ForMember(d => d.Date, o => o.MapFrom(s => s.CreatedAt.ToString("yyyy-MM-dd")))
                .ForMember(d => d.Count, o => o.Ignore());

            CreateMap<JobPost, ReturnJobDto>()
                .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.EmployerProfile!.CompanyName))
                .ForMember(d => d.ApplicationCount, o => o.MapFrom(s => s.Applications.Count));

            CreateMap<JobApplication, ReturnJobApplicationDto>()
                .ForMember(d => d.JobTitle, o => o.MapFrom(s => s.JobPost!.Title))
                .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.JobPost!.EmployerProfile!.CompanyName))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            CreateMap<EmployerProfile, ReturnEmployerDto>();
            CreateMap<JobSeekerProfile, ReturnJobSeekerDto>();
        }
    }
}