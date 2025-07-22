namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class ProjectMetricsDto
    {
        public int Bugs { get; set; }
        public int Vulnerabilities { get; set; }
        public int CodeSmells { get; set; }
        public int SecurityHotspots { get; set; }
        public int DuplicatedLines { get; set; }
        public int DuplicatedBlocks { get; set; }
        public double DuplicatedLinesDensity { get; set; }
        public double Coverage { get; set; }
    }
}
