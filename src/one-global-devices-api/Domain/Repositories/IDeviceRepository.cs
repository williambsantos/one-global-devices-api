using OneGlobalDevicesApi.Domain.Entities;
using System.Data.Common;

namespace OneGlobalDevicesApi.Domain.Repositories
{
    public interface IDeviceRepository
    {
        Task<int> SaveAsync(DeviceEntity entity, CancellationToken cancellationToken = default);
        
        Task<int> UpdateAsync(DeviceEntity entity,
            DbConnection connection, DbTransaction transaction,
            CancellationToken cancellationToken = default
        );

        Task<int> DeleteAsync(Guid deviceId,
            DbConnection connection, DbTransaction transaction,
            CancellationToken cancellationToken = default
        );

        Task<DeviceEntity?> FetchByIdAsync(Guid deviceId, 
            DbConnection connection, DbTransaction transaction,
            CancellationToken cancellationToken = default
        );

        Task<DeviceEntity?> FetchByIdAsync(Guid deviceId, CancellationToken cancellationToken = default);

        Task<IEnumerable<DeviceEntity>> FetchAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<DeviceEntity>> FetchAllByBrandAsync(string deviceBrand, CancellationToken cancellationToken = default);
        Task<IEnumerable<DeviceEntity>> FetchAllByStateAsync(DeviceStateEnum deviceState, CancellationToken cancellationToken = default);
    }
}
