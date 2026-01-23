namespace OneGlobalDevicesApi.Domain.Entities
{
    public record PaginationRequest
    {
        public static PaginationRequest Default = new PaginationRequest();

        public const int DefaultOffset = 0;
        public const int DefaultPageSize = 100;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 100;


        public int GetOffset() => (PageNumber - 1) * PageSize;
        public int GetLimit() => PageSize;
    }
}
