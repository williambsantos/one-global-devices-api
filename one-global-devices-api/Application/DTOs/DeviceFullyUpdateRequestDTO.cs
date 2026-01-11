using OneGlobalDevicesApi.Domain.Entities;

namespace OneGlobalDevicesApi.Application.DTOs
{
    public class DeviceFullyUpdateRequestDTO
    {
        public required string NewName { get; set; }
        public required string NewBrand { get; set; }
        public required DeviceStateEnum NewState { get; set; }
    }
}
