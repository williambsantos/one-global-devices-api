namespace OneGlobalDevicesApi.Domain.Entities
{
    public record PaginationResponse<T>
    {
        public PaginationResponse() : this(0, new PaginationRequest(), Enumerable.Empty<T>())
        {
        }

        public PaginationResponse(int totalElements, PaginationRequest paginationRequest, IEnumerable<T> list)
        {
            Content = list;
            Pageable = paginationRequest;
            TotalElements = totalElements;
            TotalPages = (int)Math.Ceiling((double)totalElements / paginationRequest.PageSize);
            NumberOfElements = list.Count();
            First = paginationRequest.PageNumber == 1;
            Last = paginationRequest.PageNumber >= this.TotalPages;
            Empty = !list.Any();
        }

        public IEnumerable<T> Content { get; set; } = [];
        public PaginationRequest Pageable { get; set; } = new PaginationRequest();
        public int TotalElements { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
        public int NumberOfElements { get; set; } = 0;
        public bool First { get; set; } = true;
        public bool Last { get; set; } = true;
        public bool Empty { get; set; } = true;
    }

    public static class PaginationResponseExtensions
    {
        public static PaginationResponse<TDestination> ConvertContentTo<TSource, TDestination>(
            this PaginationResponse<TSource> source,
            Func<TSource, TDestination> fnConvertMethod)
        {
            ArgumentNullException.ThrowIfNull(source);

            var destination = new PaginationResponse<TDestination>
            {
                Pageable = source.Pageable,
                TotalElements = source.TotalElements,
                TotalPages = source.TotalPages,
                NumberOfElements = source.NumberOfElements,
                First = source.First,
                Last = source.Last,
                Empty = source.Empty,
                Content = source.Content.Select(fnConvertMethod)
            };

            return destination;
        }
    }
}
