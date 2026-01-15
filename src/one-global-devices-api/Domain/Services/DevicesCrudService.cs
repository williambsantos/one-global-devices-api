using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Exceptions;
using OneGlobalDevicesApi.Domain.Repositories;
using OneGlobalDevicesApi.Infra.SQLServer.Connections;
using System.Data;
using System.Data.Common;

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

        Task<IEnumerable<DeviceEntity>> FetchAllDevicesAsync(CancellationToken cancellationToken);

        Task<IEnumerable<DeviceEntity>> FetchAllDevicesByBrandAsync(string deviceBrand, CancellationToken cancellationToken);

        Task<IEnumerable<DeviceEntity>> FetchAllDevicesByStateAsync(DeviceStateEnum deviceState, CancellationToken cancellationToken);
    }

    public class DevicesCrudService : IDevicesCrudService
    {
        public readonly IDeviceRepository _deviceRepository;
        private readonly IDatabaseConnection _databaseConnection;

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
            // Check if there are changes
            if (newName == null && 
                newBrand == null && 
                newState == null
                )
            {
                throw new DeviceBusinessException("At least one field (name, brand, state) must be provided for update.");
            }

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

            // defensive program
            currentDevice.Name ??= string.Empty;
            currentDevice.Brand ??= string.Empty;

            // check updates. If null, keep current value
            newName ??= currentDevice.Name;
            newBrand ??= currentDevice.Brand;
            newState ??= currentDevice.State;

            if (currentDevice.State == newState &&
                currentDevice.Name.Equals(newName, StringComparison.InvariantCultureIgnoreCase) &&
                currentDevice.Brand.Equals(newBrand, StringComparison.InvariantCultureIgnoreCase))
            {
                // there are no changes, return information as if updated
                return currentDevice;
            }

            // Check rules to update device in Use
            if (currentDevice.State == DeviceStateEnum.InUse)
            {
                // Check if there are changes at name
                if (currentDevice.Name != newName)
                {
                    throw new DeviceBusinessException("Cannot update device name while it is In Use.");
                }

                if (currentDevice.Brand != newBrand)
                {
                    throw new DeviceBusinessException("Cannot update device brand while it is In Use.");
                }
            }

            // prepare entity to update at repository
            currentDevice.Name = newName;
            currentDevice.Brand = newBrand;
            currentDevice.State = newState.Value;

            await _deviceRepository.UpdateAsync(
                currentDevice,
                connection, transaction,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);
            
            return currentDevice;
        }

        #endregion

        #region Delete a single device.

        public async Task DeleteSingleDeviceAsync(Guid id, CancellationToken cancellationToken)
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

        #endregion

        #region Fetch Methods

        public async Task<DeviceEntity?> FetchSingleDeviceAsync(Guid id, CancellationToken cancellationToken) =>
            await _deviceRepository.FetchByIdAsync(id, cancellationToken);

        public async Task<IEnumerable<DeviceEntity>> FetchAllDevicesAsync(CancellationToken cancellationToken) =>
            await _deviceRepository.FetchAllAsync(cancellationToken);

        public async Task<IEnumerable<DeviceEntity>> FetchAllDevicesByBrandAsync(string deviceBrand, CancellationToken cancellationToken) =>
            await _deviceRepository.FetchAllByBrandAsync(deviceBrand, cancellationToken);

        public async Task<IEnumerable<DeviceEntity>> FetchAllDevicesByStateAsync(DeviceStateEnum deviceState, CancellationToken cancellationToken) =>
            await _deviceRepository.FetchAllByStateAsync(deviceState, cancellationToken);

        #endregion
    }
}
