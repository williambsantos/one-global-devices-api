namespace OneGlobalDevicesApi.Domain.Entities
{
    public class DeviceEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required string Brand { get; set; }
        
        public required DeviceStateEnum State = DeviceStateEnum.Available;

        /// <summary>
        /// Creation time cannot be updated
        /// </summary>
        public DateTime CreationTime { get; init; } = DateTime.UtcNow;
    }
}
