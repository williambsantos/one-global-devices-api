using OneGlobalDevicesApi.Domain.Entities;

namespace OneGlobalDevicesApi.Application.DTOs
{
    public class PaginationRequestDTO
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 100;
    }

    public static class PaginationRequestDTOExtensions
    {
        public static PaginationRequest ToDomainEntity(this PaginationRequestDTO dto)
        {
            if (dto == null)
                return new PaginationRequest();

            return new PaginationRequest
            {
                PageNumber = dto.Page,
                PageSize = dto.Size
            };
        }
    }
}
