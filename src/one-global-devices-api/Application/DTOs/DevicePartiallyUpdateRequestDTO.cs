using OneGlobalDevicesApi.Domain.Entities;

namespace OneGlobalDevicesApi.Application.DTOs
{
    public class DevicePartiallyUpdateRequestDTO
    {
        public string? NewName { get; set; }
        public string? NewBrand { get; set; }
        public DeviceStateEnum? NewState { get; set; }
    }
}
