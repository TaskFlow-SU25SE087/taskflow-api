using AutoMapper;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Mappings
{
    public class ProjectMapper : Profile
    {
        public ProjectMapper()
        {
            CreateMap<Project, ProjectsResponse>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    if (context.Items.TryGetValue("Members", out var membersObj) && context.Items.TryGetValue("UserId", out var userIdObj))
                    {
                        var members = (List<ProjectMember>)membersObj;
                        var userId = (Guid)userIdObj;
                        return members.FirstOrDefault(m => m.ProjectId == src.Id && m.UserId == userId)?.Role;
                    }
                    return null;
                }))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.LastUpdate, opt => opt.MapFrom(src => src.LastUpdate));

            CreateMap<Project, OneProjectResponse>();
            CreateMap<Project, ProjectResponse>();
        }

    }
}
