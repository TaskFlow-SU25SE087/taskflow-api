namespace taskflow_api.Model.Common
{
    public class PagingParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize => 3;

        public int Skip => (PageNumber - 1) * PageSize;
    }
}
