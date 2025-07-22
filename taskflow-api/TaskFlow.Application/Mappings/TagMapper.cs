using AutoMapper;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Mappings
{
    public class TagMapper : Profile
    {
        public TagMapper()
        {
            CreateMap<Tag, TagResporn>();
        }
    }
}
