using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Shared.Helpers;

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

        public async Task DeleteProjectSonar(string projectKey)
        {
            var sonarHost = _config["SonarQube:HostUrl"];
            var sonarToken = _config["SonarQube:Token"];

            using var request = new HttpRequestMessage(HttpMethod.Post, 
                $"{sonarHost}/api/projects/delete?project={projectKey}");

            if (!string.IsNullOrEmpty(sonarToken))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{sonarToken}:");
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete project {projectKey}. " +
                                    $"Status: {response.StatusCode}, Response: {content}");
            }
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

        public async Task<ProjectMetricsDto> GetProjectMeasuresAsync(string projectKey)
        {
            var client = new HttpClient();
            var metrics = "bugs,vulnerabilities,code_smells,coverage,duplicated_lines,duplicated_blocks,duplicated_lines_density,security_hotspots";
            var url = $"{_sonarSetting.HostUrl}/api/measures/component?component={projectKey}&metricKeys={metrics}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var sonarToken = _sonarSetting.Token;
            if (!string.IsNullOrEmpty(sonarToken))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{sonarToken}:");
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            var measures = json.RootElement.GetProperty("component").GetProperty("measures");

            var dto = new ProjectMetricsDto();
            foreach (var measure in measures.EnumerateArray())
            {
                var metric = measure.GetProperty("metric").GetString();
                var valueStr = measure.GetProperty("value").GetString();
                double.TryParse(valueStr, out var value);

                switch (metric)
                {
                    case "bugs": dto.Bugs = (int)value; break;
                    case "vulnerabilities": dto.Vulnerabilities = (int)value; break;
                    case "code_smells": dto.CodeSmells = (int)value; break;
                    case "coverage": dto.Coverage = value; break;
                    case "duplicated_lines": dto.DuplicatedLines = (int)value; break;
                    case "duplicated_blocks": dto.DuplicatedBlocks = (int)value; break;
                    case "duplicated_lines_density": dto.DuplicatedLinesDensity = value; break;
                    case "security_hotspots": dto.SecurityHotspots = (int)value; break;
                }
            }
            return dto;
        }

        public async Task<string> GetQualityGateStatusAsync(string projectKey)
        {
            var client = new HttpClient();
            var url = $"{_sonarSetting.HostUrl}/api/qualitygates/project_status?projectKey={projectKey}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var sonarToken = _sonarSetting.Token;
            if (!string.IsNullOrEmpty(sonarToken))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{sonarToken}:");
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            var status = json.RootElement.GetProperty("projectStatus").GetProperty("status").GetString();
            return status ?? "UNKNOWN";
        }

        public async Task<CommitScanResult> ScanCommit(string extractPath, string projectKey, ProgrammingLanguage language, Framework framework)
        {
            var languageKey = LanguageMap.GetToolKey(language);
            if (string.IsNullOrEmpty(languageKey))
            {
                throw new ArgumentException($"Unsupported programming language: {language}");
            }
            var sonarPropsPath = Path.Combine(extractPath, "sonar-project.properties");
            await File.WriteAllTextAsync(sonarPropsPath, $@"
            sonar.projectKey={projectKey}
            sonar.sources=.
            sonar.host.url={_sonarSetting.HostUrl}
            sonar.login={_sonarSetting.Token}
            sonar.sourceEncoding=UTF-8
            sonar.exclusions=**/bin/**,**/obj/**,**/node_modules/**,**/dist/**,**/build/**,**/.git/**/*
            sonar.inclusions=**/*.java,**/*.cs,**/*.js,**/*.ts,**/*.py
            sonar.java.binaries=.
            sonar.scm.provider=git
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