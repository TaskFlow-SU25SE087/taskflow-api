using AutoMapper;
using System.Reflection.Emit;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _TagRepository;
        private readonly IMapper _mapper;

        public TagService(ITagRepository TagRepository, IMapper mapper)
        {
            _TagRepository = TagRepository;
            _mapper = mapper;
        }

        public async Task AddTag(AddTagRequest request)
        {
            var newTag = new Tag
            {
                Name = request.Name,
                ProjectId = request.ProjectId,
                Description = request.Description
            };
            await _TagRepository.AddTagAsync(newTag);
        }

        public async Task DeleteTag(Guid TagId)
        {
            if (TagId == Guid.Empty)
            {
                throw new AppException(ErrorCode.TagIsNull);
            }
            //check Task if Tag exists

            //code to check if Tag exists in tasks can be added here if needed

            var TagDelete = await _TagRepository.GetTagByIdAsync(TagId);
            if (TagDelete == null)
            {
                throw new AppException(ErrorCode.TagNotFound);
            }
            // Delete Tag
            TagDelete.IsActive = false; // Soft delete
            await _TagRepository.UpdateTagAsync(TagDelete);
        }

        public async Task<List<TagResporn>> GetListTag(Guid ProjectId)
        {
            var listTags = await _TagRepository.GetListTagsync(ProjectId);
            var result = _mapper.Map<List<TagResporn>>(listTags);
            return result;
        }

        public async Task UpdateTag(UpdateTagRequest request)
        {
            if (request.Id == Guid.Empty)
            {
                throw new AppException(ErrorCode.TagIsNull);
            }
            // Check if Tag exists
            var TagUpdate = await _TagRepository.GetTagByIdAsync(request.Id);
            if (TagUpdate == null)
            {
                throw new AppException(ErrorCode.TagNotFound);
            }
            await _TagRepository.UpdateTagAsync(TagUpdate);
        }
    }
}
