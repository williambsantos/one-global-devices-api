using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Exceptions;
using OneGlobalDevicesApi.Domain.Repositories;

namespace OneGlobalDevicesApi.Domain.Services
{
    /// <summary>
    ///  Create a new device.
    ///  Fully and/or par ally update an existing device.
    ///  Fetch a single device. 
    ///  Fetch all devices.
    ///  Fetch devices by brand. 
    ///  Fetch devices by state. 
    ///  Delete a single device. 
    /// </summary>
    public interface IDevicesCrudService
    {
        Task<DeviceEntity> CreateNewDeviceAsync(string name, string brand, CancellationToken cancellationToken);

        Task<DeviceEntity> UpdateDeviceAsync(Guid id, string? newName, string? newBrand, DeviceStateEnum? newState, CancellationToken cancellationToken);

        Task DeleteSingleDeviceAsync(Guid id, CancellationToken cancellationToken);

        Task<DeviceEntity?> FetchSingleDeviceAsync(Guid id, CancellationToken cancellationToken);

        Task<PaginationResponse<DeviceEntity>> FetchAllDevicesAsync(
            string? brand, DeviceStateEnum? state,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default
        );
    }

    public class DevicesCrudService : IDevicesCrudService
    {
        public readonly IDeviceRepository _deviceRepository;
        private readonly IDatabaseConnection _databaseConnection;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(
            initialCount: 1,
            maxCount: 1
        );

        public DevicesCrudService(
            IDeviceRepository deviceRepository,
            IDatabaseConnection databaseConnection
            )
        {
            _deviceRepository = deviceRepository;
            _databaseConnection = databaseConnection;
        }

        #region Create a new device.

        /// <summary>
        /// Create a new device
        /// </summary>
        /// <param name="name"></param>
        /// <param name="brand"></param>
        /// <returns>Device created with ID</returns>
        public async Task<DeviceEntity> CreateNewDeviceAsync(string name, string brand,
            CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var device = new DeviceEntity
                {
                    Name = name,
                    Brand = brand,
                    State = DeviceStateEnum.Available
                };

                // TODO: check with the team Business Rules about the device creation, like as unique name
                //if (string.IsNullOrWhiteSpace(name))
                //    throw new DeviceBusinessException("Device name cannot be null or empty.");

                //if (string.IsNullOrWhiteSpace(brand))
                //    throw new DeviceBusinessException("Device brand cannot be null or empty.");

                await _deviceRepository.SaveAsync(device, cancellationToken);

                return device;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion

        #region Fully and/or par ally update an existing device. 

        /// <summary>
        /// Update Device
        /// If newName is null, will not update the name.
        /// If newBrand is null, will not update the brand.
        /// If newState is null, will not update the state.
        /// </summary>
        /// <param name="id">DeviceId to Update</param>
        /// <param name="newName"></param>
        /// <param name="newBrand"></param>
        /// <param name="newState"></param>
        /// <returns>DeviceEntity updated</returns>
        /// <exception cref="DeviceBusinessException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<DeviceEntity> UpdateDeviceAsync(Guid id,
            string? newName, string? newBrand, DeviceStateEnum? newState,
            CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                ValidateUpdateRequest(newName, newBrand, newState);

                using var databaseWork = await _databaseConnection.
                    CreateConnectionAndTransactionAsync(cancellationToken);

                var connection = databaseWork.Connection;
                var transaction = databaseWork.Transaction;

                DeviceEntity? currentDevice = await _deviceRepository.FetchByIdAsync(
                    deviceId: id,
                    connection: connection,
                    transaction: transaction,
                    cancellationToken: cancellationToken
                );

                if (currentDevice == null)
                    throw new KeyNotFoundException($"Device with ID {id} not found.");

                var (finalName, finalBrand, finalState) = PrepareToChanges(
                    currentDevice, newName, newBrand, newState
                );

                // there are no changes, return information as if updated
                if (NoChangesDetected(currentDevice, finalName, finalBrand, finalState))
                    return currentDevice;

                // Check rules to update device in Use
                ValidateUpdateBusinessRules(currentDevice, finalName, finalBrand);

                // prepare entity to update at repository
                currentDevice.Name = finalName;
                currentDevice.Brand = finalBrand;
                currentDevice.State = finalState;

                await _deviceRepository.UpdateAsync(
                    currentDevice,
                    connection, transaction,
                    cancellationToken
                );

                await transaction.CommitAsync(cancellationToken);

                return currentDevice;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static void ValidateUpdateRequest(string? newName, string? newBrand, DeviceStateEnum? newState)
        {
            if (newName == null && newBrand == null && newState == null)
                throw new DeviceBusinessException("At least one field must be provided for update.");
        }

        private static (string finalName, string finalBrand, DeviceStateEnum finalState) PrepareToChanges(
            DeviceEntity currentDevice, string? newName, string? newBrand, DeviceStateEnum? newState)
        {
            // check updates. If null, keep current value
            var finalName = newName ?? currentDevice?.Name ?? string.Empty;
            var finalBrand = newBrand ?? currentDevice?.Brand ?? string.Empty;
            var finalState = newState ?? currentDevice?.State ?? default;

            return (finalName, finalBrand, finalState);
        }

        private static void ValidateUpdateBusinessRules(DeviceEntity currentDevice, string newName, string newBrand)
        {
            if (currentDevice.State == DeviceStateEnum.InUse)
            {
                if (currentDevice.Name != newName)
                    throw new DeviceBusinessException("Cannot update device name while it is In Use.");
                if (currentDevice.Brand != newBrand)
                    throw new DeviceBusinessException("Cannot update device brand while it is In Use.");
            }
        }

        private static bool NoChangesDetected(DeviceEntity currentDevice, string finalName, string finalBrand, DeviceStateEnum finalState)
        {
            return currentDevice.State == finalState &&
                   currentDevice.Name.Equals(finalName, StringComparison.InvariantCultureIgnoreCase) &&
                   currentDevice.Brand.Equals(finalBrand, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

        #region Delete a single device.

        public async Task DeleteSingleDeviceAsync(Guid id, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                using var databaseWork = await _databaseConnection.CreateConnectionAndTransactionAsync(cancellationToken);

                var connection = databaseWork.Connection;
                var transaction = databaseWork.Transaction;

                DeviceEntity? currentDevice = await _deviceRepository.FetchByIdAsync(
                    deviceId: id,
                    connection: connection,
                    transaction: transaction,
                    cancellationToken: cancellationToken
                );
                if (currentDevice == null)
                {
                    throw new KeyNotFoundException($"Device with ID {id} not found.");
                }

                // Check rules to update device in Use
                if (currentDevice.State == DeviceStateEnum.InUse)
                {
                    throw new DeviceBusinessException("Cannot delete device while it is In Use.");
                }

                await _deviceRepository.DeleteAsync(id,
                    connection,
                    transaction,
                    cancellationToken
                );

                await transaction.CommitAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion

        #region Fetch Methods

        public async Task<DeviceEntity?> FetchSingleDeviceAsync(Guid id, CancellationToken cancellationToken) =>
            await _deviceRepository.FetchByIdAsync(id, cancellationToken);

        public async Task<PaginationResponse<DeviceEntity>> FetchAllDevicesAsync(
            string? brand, DeviceStateEnum? state,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default) =>
                await _deviceRepository.FetchAllAsync(
                    brand, state, paginationRequest, cancellationToken
                );

        #endregion
    }
}
