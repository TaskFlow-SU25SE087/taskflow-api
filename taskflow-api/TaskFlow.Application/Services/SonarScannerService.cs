using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class SonarScannerService : ICodeScanService
    {
        private readonly SonarQubeSetting _sonarSetting;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public SonarScannerService(IOptions<SonarQubeSetting> sonarSetting,
            HttpClient httpClient, IConfiguration config)
        {
            _sonarSetting = sonarSetting.Value;
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<SonarIssueResponse>> GetIssuesByProjectAsync(string projectKey)
        {
            var sonarHost = _config["SonarQube:HostUrl"];
            var sonarToken = _config["SonarQube:Token"];

            var request = new HttpRequestMessage(HttpMethod.Get,
            $"{sonarHost}/api/issues/search?projectKeys={projectKey}");

            if (!string.IsNullOrEmpty(sonarToken))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{sonarToken}:");
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            var issues = new List<SonarIssueResponse>();
            foreach (var issue in json.RootElement.GetProperty("issues").EnumerateArray())
            {
                issues.Add(new SonarIssueResponse
                {
                    Key = issue.GetProperty("key").GetString() ?? "",
                    Rule = issue.GetProperty("rule").GetString() ?? "",
                    Severity = issue.GetProperty("severity").GetString() ?? "",
                    Component = issue.GetProperty("component").GetString() ?? "",
                    Line = issue.TryGetProperty("line", out var lineProp) ? lineProp.GetInt32() : 0,
                    Message = issue.GetProperty("message").GetString() ?? ""
                });
            }
            return issues;
        }

        public async Task<CommitScanResult> ScanCommit(string extractPath, string projectKey, ProgrammingLanguage language, Framework framework)
        {
            var sonarPropsPath = Path.Combine(extractPath, "sonar-project.properties");
            await File.WriteAllTextAsync(sonarPropsPath, $@"
            sonar.projectKey={projectKey}
            sonar.sources=.
            sonar.host.url={_sonarSetting.HostUrl}
            sonar.login={_sonarSetting.Token}
            sonar.sourceEncoding=UTF-8
            sonar.exclusions=**/bin/**,**/obj/**,**/node_modules/**
            sonar.languages=csharp,java,python
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
                ErrorLog = error,
                ProjectKey = projectKey
            };

            return reresult;
        }
    }
}