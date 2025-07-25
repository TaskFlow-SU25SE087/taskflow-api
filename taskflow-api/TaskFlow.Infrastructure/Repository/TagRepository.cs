﻿using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly TaskFlowDbContext _context;

        public TagRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task AddTagAsync(Tag Tag)
        {
            await _context.Tags.AddAsync(Tag);
            await _context.SaveChangesAsync();
        }

        public Task<bool> CheckNameTagAsync(Guid ProjectId, string Name)
        {
            return _context.Tags
                .AnyAsync(t => t.ProjectId == ProjectId && t.Name == Name && t.IsActive);
        }

        public async Task DeleteTagAsync(Guid TagId)
        {
            var TagDelete = await _context.Tags.FindAsync(TagId);
            _context.Tags.Remove(TagDelete!);
                await _context.SaveChangesAsync();
        }

        public async Task<List<Tag>> GetListTagsync(Guid ProjectId)
        {
            return await _context.Tags
                .Where(t => t.ProjectId == ProjectId && t.IsActive)
                .ToListAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(Guid TagId)
        {
            return await _context.Tags
                    .FirstOrDefaultAsync(x => x.Id == TagId);
        }

        public async Task UpdateTagAsync(Tag Tag)
        {
            _context.Tags.Update(Tag);
            await _context.SaveChangesAsync();
        }
    }
}
