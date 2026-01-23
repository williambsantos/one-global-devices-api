namespace OneGlobalDevicesApi.Domain.Entities
{
    public record PaginationResponse<T>
    {
        public PaginationResponse() : this(0, new PaginationRequest(), Enumerable.Empty<T>())
        {
        }

        public PaginationResponse(int totalElements, PaginationRequest paginationRequest, IEnumerable<T> list)
        {
            FillFrom(totalElements, paginationRequest, list);
        }

        public IEnumerable<T> Content { get; set; } = [];
        public PaginationRequest Pageable { get; set; } = new PaginationRequest();
        public int TotalElements { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
        public int NumberOfElements { get; set; } = 0;
        public bool First { get; set; } = true;
        public bool Last { get; set; } = true;
        public bool Empty { get; set; } = true;

        internal void ConvertContent<TEntity>(PaginationResponse<TEntity> entityData, Func<TEntity, T> fnConvertMethod)
        {
            if (entityData == null)
                return;

            this.Pageable = entityData.Pageable;
            this.TotalElements = entityData.TotalElements;
            this.TotalPages = entityData.TotalPages;
            this.NumberOfElements = entityData.NumberOfElements;
            this.First = entityData.First;
            this.Last = entityData.Last;
            this.Empty = entityData.Empty;
            this.Content = entityData.Content.Select(fnConvertMethod);
        }

        public void FillFrom(int totalElements, PaginationRequest paginationRequest, IEnumerable<T> list)
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
    }
}
