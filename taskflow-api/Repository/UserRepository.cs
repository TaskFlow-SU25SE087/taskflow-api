using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using taskflow_api.Data;
using taskflow_api.Entity;
using taskflow_api.Model;

namespace taskflow_api.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskFlowDbContext context;

        public UserRepository(TaskFlowDbContext context)
        {
            this.context = context;
        }

    }
}
