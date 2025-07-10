using Microsoft.Extensions.Options;
using System.Diagnostics;
using taskflow_api.TaskFlow.Application.DTOs.Common.Attributes;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class SonarScannerService : ICodeScanService
    {
        private readonly SonarQubeSetting _sonarSetting;

        public SonarScannerService(IOptions<SonarQubeSetting> sonarSetting)
        {
            _sonarSetting = sonarSetting.Value;
        }
        public async Task ScanCommit(string extractPath, string projectKey)
        {
            var sonarPropsPath = Path.Combine(extractPath, "sonar-project.properties");
            await File.WriteAllTextAsync(sonarPropsPath, $@"
            sonar.projectKey={projectKey}
            sonar.sources=.
            sonar.host.url={_sonarSetting.HostUrl}
            sonar.login={_sonarSetting.Token}
            ");

            var process = new ProcessStartInfo
            {
                FileName = "sonar-scanner",
                WorkingDirectory = extractPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var proc = Process.Start(process);
            await proc.WaitForExitAsync();
        }
    }
}
