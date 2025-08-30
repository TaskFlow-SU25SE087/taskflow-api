
using System;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class MidnightJobService : BackgroundService
    {
        private readonly ILogger<MidnightJobService> _logger;
        private readonly AppTimeProvider _timeProvider;

        public MidnightJobService(ILogger<MidnightJobService> logger, AppTimeProvider timeProvider)
        {
            _logger = logger;
            _timeProvider = timeProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var Now = _timeProvider.Now;
                var nextMidnight = Now.Date.AddDays(1);
                var delay = nextMidnight - Now;

                _logger.LogInformation($"MidnightJobService sleeping for {delay.TotalMinutes} minutes until {nextMidnight}.");

                await Task.Delay( delay );
                try
                {
                    // Place your midnight job logic here
                    _logger.LogInformation("Midnight job executed at: {time}", DateTimeOffset.Now);

                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing midnight job.");
                }
            }
        }
    }
}
