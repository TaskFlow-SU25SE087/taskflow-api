using Microsoft.Extensions.Options;
using System.Diagnostics;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
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
        public async Task<CommitScanResult> ScanCommit(string extractPath, string projectKey)
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
                FileName = _sonarSetting.ScannerPath,
                WorkingDirectory = extractPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var proc = Process.Start(process)!;
            var output = await proc.StandardOutput.ReadToEndAsync();
            var error = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            var reresult = new CommitScanResult
            {
                Success = proc.ExitCode == 0,
                OutputLog = output,
                ErrorLog = error
            };

            return reresult;
        }
    }
}
