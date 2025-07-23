using Microsoft.Extensions.Options;
using taskflow_api.TaskFlow.Application.DTOs.Common.Attributes;

namespace taskflow_api.TaskFlow.Shared.Helpers
{
    public class AppTimeProvider
    {
        private readonly TimeSpan _offset;

        public AppTimeProvider(IOptions<TimeSettings> settings)
        {
            _offset = TimeSpan.FromHours(settings.Value.UtcOffsetHours);
        }

        public DateTime Now => DateTime.UtcNow.Add(_offset);
        public DateTime UtcNow => DateTime.UtcNow;

    }
}
