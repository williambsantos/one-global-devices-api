using OneGlobalDevicesApi.Domain.Entities;

namespace OneGlobalDevicesApi.Domain.Repositories
{
    public interface IDeviceRepository
    {
        Task SaveAsync(DeviceEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(DeviceEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid deviceId, CancellationToken cancellationToken = default);
        Task<DeviceEntity?> FetchByIdAsync(Guid deviceId, CancellationToken cancellationToken = default);

        Task<IEnumerable<DeviceEntity>> FetchAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<DeviceEntity>> FetchAllByBrandAsync(string deviceBrand, CancellationToken cancellationToken = default);
        Task<IEnumerable<DeviceEntity>> FetchAllByStateAsync(DeviceStateEnum deviceState, CancellationToken cancellationToken = default);
    }
}
