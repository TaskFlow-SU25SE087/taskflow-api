using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace taskflow_api.TaskFlow.Infrastructure.Data
{
    public class TaskFlowDbContextFactory : IDesignTimeDbContextFactory<TaskFlowDbContext>
    {
        public TaskFlowDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TaskFlowDbContext>();
            var connectionString = configuration.GetConnectionString("dbApp");
            optionsBuilder.UseSqlServer(connectionString);

            return new TaskFlowDbContext(optionsBuilder.Options);
        }
    }
}
