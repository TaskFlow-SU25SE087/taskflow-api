using System.Reflection.Emit;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;

        public LabelService(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository;
        }

        public async Task AddLabel(AddLabelRequest request)
        {
            var newLabel = new Labels
            {
                Name = request.Name,
                ProjectId = request.ProjectId,
                Description = request.Description
            };
            await _labelRepository.AddLabelAsync(newLabel);
        }

        public async Task DeleteLabel(Guid labelId)
        {
            if (labelId == Guid.Empty)
            {
                throw new AppException(ErrorCode.LabelIsNull);
            }
            //check Task if label exists

            //code to check if label exists in tasks can be added here if needed

            var labelDelete = await _labelRepository.GetLabelByIdAsync(labelId);
            if (labelDelete == null)
            {
                throw new AppException(ErrorCode.LabelNotFound);
            }
            // Delete label
            labelDelete.IsActive = false; // Soft delete
            await _labelRepository.UpdateLabelAsync(labelDelete);
        }

        public async Task UpdateLabel(UpdateLabelRequest request)
        {
            if (request.Id == Guid.Empty)
            {
                throw new AppException(ErrorCode.LabelIsNull);
            }
            // Check if label exists
            var labelUpdate = await _labelRepository.GetLabelByIdAsync(request.Id);
            if (labelUpdate == null)
            {
                throw new AppException(ErrorCode.LabelNotFound);
            }
            await _labelRepository.UpdateLabelAsync(labelUpdate);
        }
    }
}
