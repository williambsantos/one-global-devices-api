namespace OneGlobalDevicesApi.Domain.Entities
{
    public class DeviceEntity
    {
        public static Guid CreateGuid() => Guid.CreateVersion7();

        public Guid Id { get; init; } = CreateGuid();
        public required string Name { get; set; }
        public required string Brand { get; set; }

        /// <summary>
        /// TODO: check the rule for initial device state
        /// </summary>
        public DeviceStateEnum State { get; set; } = DeviceStateEnum.Available;

        /// <summary>
        /// Creation time cannot be updated
        /// </summary>
        public DateTimeOffset CreationTime { get; init; } = DateTimeOffset.UtcNow;
    }
}
