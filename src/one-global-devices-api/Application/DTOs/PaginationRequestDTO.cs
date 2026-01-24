using OneGlobalDevicesApi.Domain.Entities;

namespace OneGlobalDevicesApi.Application.DTOs
{
    public class PaginationRequestDTO
    {
        public int? Page { get; set; } = 1;
        public int? Size { get; set; } = 100;
    }

    public static class PaginationRequestDTOExtensions
    {
        public static PaginationRequest ToDomainEntity(this PaginationRequestDTO dto)
        {
            if (dto == null)
                return new PaginationRequest();

            if (dto.Page <= 0)
                dto.Page = 1;

            return new PaginationRequest
            {
                PageNumber = dto.Page ?? 1,
                PageSize = dto.Size ?? 100
            };
        }
    }
}
