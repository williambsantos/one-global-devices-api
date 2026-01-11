using OneGlobalDevicesApi.Domain.Entities;

namespace OneGlobalDevicesApi.Domain.Repositories
{
    public interface IDeviceRepository
    {
        Task SaveAsync(DeviceEntity entity);
        Task UpdateAsync(DeviceEntity entity);
        Task DeleteAsync(Guid deviceId);
        Task<DeviceEntity> FetchByIdAsync(Guid deviceId);

        Task<IEnumerable<DeviceEntity>> FetchAllAsync();
        Task<IEnumerable<DeviceEntity>> FetchAllByBrandAsync(string deviceBrand);
        Task<IEnumerable<DeviceEntity>> FetchAllByStateAsync(DeviceStateEnum deviceState);
    }
}
