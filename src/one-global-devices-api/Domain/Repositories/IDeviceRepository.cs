using System.Data.Common;
using OneGlobalDevicesApi.Domain.Entities;

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

        Task<PaginationResponse<DeviceEntity>> FetchAllAsync(
            string? brand, DeviceStateEnum? state,
            PaginationRequest paginationRequest, CancellationToken cancellationToken = default
        );
    }
}
