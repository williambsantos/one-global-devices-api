using OneGlobalDevicesApi.Domain.Entities;

namespace OneGlobalDevicesApi.Application.DTOs
{
    public class DeviceResponseDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Brand { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public DateTimeOffset CreationTime { get; init; }

        public DeviceResponseDto(DeviceEntity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Brand = entity.Brand;
            State = entity.State.ToString();
            CreationTime = entity.CreationTime;
        }
    }
}
