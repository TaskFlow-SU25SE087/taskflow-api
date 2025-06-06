namespace taskflow_api.TaskFlow.Application.DTOs.Common
{
    public class PagingParams
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int Skip => (PageNumber - 1) * PageSize;
    }
}
