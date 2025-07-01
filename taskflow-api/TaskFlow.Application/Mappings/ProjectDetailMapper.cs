using AutoMapper;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Mappings
{
    public class ProjectDetailMapper : Profile
    {
        public ProjectDetailMapper()
        {
            CreateMap<Project, ProjectDetailResponse>();

            CreateMap<Board, BoardDetailResponse>()
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.TaskProject));

            CreateMap<TaskProject, TaskDetailResponse>()
                .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Name : null))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.TaskComments))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TaskTags.Select(tl => tl.Tag)));


            CreateMap<TaskComment, CommentDetailResponse>();

            CreateMap<Tag, TagDetailResponse>();

            CreateMap<Sprint, SprintDetailResponse>();
            CreateMap<ProjectPart, ProjectPartResponse>();

        }
    }
}
