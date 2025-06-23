using AutoMapper;
using Microsoft.Extensions.Configuration;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Mappings
{
    public class TaskProjectMapper : Profile
    {
        public TaskProjectMapper()
        {
            CreateMap<TaskProject, TaskProjectResponse>()
                .ForMember(dest => dest.TaskAssignees, opt => opt.MapFrom(src => src.TaskAssignees))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TaskTags))
                .ForMember(dest => dest.commnets, opt => opt.MapFrom(src => src.TaskComments))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Board != null ? src.Board.Name : "Unknown"));

            CreateMap<TaskAssignee, TaskAssigneeResponse>()
                .ForMember(dest => dest.Executor, opt => opt.MapFrom(src => src.ProjectMember.User.FullName))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ProjectMember.User.Avatar)
                        // Fixed hardcoded URL since IConfiguration cannot be injected into AutoMapper Profile constructor
                        ? "https://res.cloudinary.com/dpw9sgxab/image/upload/v1749247007/avatar/default.jpg" 
                        : src.ProjectMember.User.Avatar))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.ProjectMember.Role));

            CreateMap<TaskTag, TaskTagResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Tag.Name))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Tag.Color))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Tag.Description));

            CreateMap<TaskComment, CommnetResponse>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src =>src.Content))
                .ForMember(dest => dest.Commenter, opt => opt.MapFrom(src =>src.UserComment.User.FullName))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src =>src.UserComment.User.Avatar))
                .ForMember(dest => dest.LastUpdate, opt => opt.MapFrom(src =>src.LastUpdatedAt));
        }
    }
}
