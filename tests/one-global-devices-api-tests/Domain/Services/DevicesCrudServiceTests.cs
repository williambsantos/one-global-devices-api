using FluentAssertions;
using Microsoft.AspNetCore.Localization;
using NSubstitute;
using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Exceptions;
using OneGlobalDevicesApi.Domain.Repositories;
using OneGlobalDevicesApi.Domain.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneGlobalDevicesApiTests.Domain.Services
{
    public class DevicesCrudServiceTests
    {
        #region Save

        [Fact]
        public async Task CreateNewDeviceAsync_Must_Fetch_And_Delete()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var name = "IPHONE 17 PRO MAX";
            var brand = "Apple";
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            DeviceEntity deviceCreated = await devicesCrudService.CreateNewDeviceAsync(
                name, brand,
                cancellationToken
            );

            // Assert
            deviceCreated.Should().NotBeNull();
            deviceCreated.Name.Should().Be(name);
            deviceCreated.Brand.Should().Be(brand);

            await deviceRepositoryMock.Received(1).SaveAsync(Arg.Any<DeviceEntity>(), cancellationToken);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteSingleDeviceAsync_Must_Fetch_And_Delete()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var connectionMock = Substitute.For<DbConnection>();
            var transactionMock = Substitute.For<DbTransaction>();
            var connection = connectionMock;
            var transaction = transactionMock;

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            deviceRepositoryMock
                .FetchByIdAsync(deviceId, connection, transaction, cancellationToken)
                .Returns(new DeviceEntity
                {
                    Id = deviceId,
                    Name = "Iphone 17 PRO MAX",
                    Brand = "Apple"
                });

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            databaseConnectionMock
                .CreateConnectionAndTransactionAsync(cancellationToken)
                .Returns(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            await devicesCrudService.DeleteSingleDeviceAsync(deviceId, cancellationToken);

            // Assert
            await deviceRepositoryMock.Received(1).FetchByIdAsync(deviceId, connection, transaction, cancellationToken);

            await deviceRepositoryMock.Received(1).DeleteAsync(deviceId, connection, transaction, cancellationToken);

            await transactionMock.Received(1).CommitAsync(cancellationToken);
        }

        [Fact]
        public async Task DeleteSingleDeviceAsync_When_NotFound_Throw_Exception()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var connectionMock = Substitute.For<DbConnection>();
            var transactionMock = Substitute.For<DbTransaction>();
            var connection = connectionMock;
            var transaction = transactionMock;

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            deviceRepositoryMock
                .FetchByIdAsync(deviceId, connection, transaction, cancellationToken)
                .Returns(new DeviceEntity
                {
                    Id = deviceId,
                    Name = "Iphone 17 PRO MAX",
                    Brand = "Apple",
                    State = DeviceStateEnum.InUse
                });

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            databaseConnectionMock
                .CreateConnectionAndTransactionAsync(cancellationToken)
                .Returns(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var act = async () => await devicesCrudService.DeleteSingleDeviceAsync(deviceId, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<DeviceBusinessException>();

            await deviceRepositoryMock.Received(1).FetchByIdAsync(deviceId, connection, transaction, cancellationToken);

            await deviceRepositoryMock.Received(0).DeleteAsync(deviceId, connection, transaction, cancellationToken);

            await transactionMock.Received(0).CommitAsync(cancellationToken);
        }

        [Fact]
        public async Task DeleteSingleDeviceAsync_When_Fetch_In_Use_Throw_Exception()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var connectionMock = Substitute.For<DbConnection>();
            var transactionMock = Substitute.For<DbTransaction>();
            var connection = connectionMock;
            var transaction = transactionMock;

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            DeviceEntity? deviceNotFound = null;

            deviceRepositoryMock
                .FetchByIdAsync(deviceId, connection, transaction, cancellationToken)
                .Returns(deviceNotFound);

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            databaseConnectionMock
                .CreateConnectionAndTransactionAsync(cancellationToken)
                .Returns(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var act = async () => await devicesCrudService.DeleteSingleDeviceAsync(deviceId, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();

            await deviceRepositoryMock.Received(1).FetchByIdAsync(deviceId, connection, transaction, cancellationToken);

            await deviceRepositoryMock.Received(0).DeleteAsync(deviceId, connection, transaction, cancellationToken);

            await transactionMock.Received(0).CommitAsync(cancellationToken);
        }

        #endregion

        #region Update

        [Fact]
        public async Task UpdateDeviceAsync_Must_Fetch_And_Update()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            var currentDevice = new DeviceEntity
            {
                Brand = "Apple",
                Name = "Iphone 17 PRO MAX",
                State = DeviceStateEnum.Available,
            };
            var deviceId = currentDevice.Id;

            var newName = currentDevice.Name + ". Change";
            var newBrand = currentDevice.Brand + ". Change";
            var newState = DeviceStateEnum.InUse;

            var connectionMock = Substitute.For<DbConnection>();
            var transactionMock = Substitute.For<DbTransaction>();
            var connection = connectionMock;
            var transaction = transactionMock;

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            deviceRepositoryMock
                .FetchByIdAsync(deviceId, connection, transaction, cancellationToken)
                .Returns(currentDevice);

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            databaseConnectionMock
                .CreateConnectionAndTransactionAsync(cancellationToken)
                .Returns(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            await devicesCrudService.UpdateDeviceAsync(
                deviceId, newName, newBrand, newState, cancellationToken
            );

            // Assert
            await deviceRepositoryMock.Received(1).FetchByIdAsync(deviceId, connection, transaction, cancellationToken);

            await deviceRepositoryMock.Received(1).UpdateAsync(currentDevice, connection, transaction, cancellationToken);

            await transactionMock.Received(1).CommitAsync(cancellationToken);
        }

        [Fact]
        public async Task UpdateDeviceAsync_When_AllData_NOT_Change_Return()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            var currentDevice = new DeviceEntity
            {
                Brand = "Apple",
                Name = "Iphone 17 PRO MAX",
                State = DeviceStateEnum.Available,
            };
            var deviceId = currentDevice.Id;

            var newName = currentDevice.Name;
            var newBrand = currentDevice.Brand;
            var newState = currentDevice.State;

            var connectionMock = Substitute.For<DbConnection>();
            var transactionMock = Substitute.For<DbTransaction>();
            var connection = connectionMock;
            var transaction = transactionMock;

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            deviceRepositoryMock
                .FetchByIdAsync(deviceId, connection, transaction, cancellationToken)
                .Returns(currentDevice);

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            databaseConnectionMock
                .CreateConnectionAndTransactionAsync(cancellationToken)
                .Returns(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            await devicesCrudService.UpdateDeviceAsync(
                deviceId, newName, newBrand, newState, cancellationToken
            );

            // Assert
            await deviceRepositoryMock.Received(1).FetchByIdAsync(deviceId, connection, transaction, cancellationToken);

            await deviceRepositoryMock.Received(0).UpdateAsync(currentDevice, connection, transaction, cancellationToken);

            await transactionMock.Received(0).CommitAsync(cancellationToken);
        }

        [Fact]
        public async Task UpdateDeviceAsync_When_AllData_Empty_Throw_Exception()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            var currentDevice = new DeviceEntity
            {
                Brand = "Apple",
                Name = "Iphone 17 PRO MAX",
                State = DeviceStateEnum.InUse,
            };
            var deviceId = currentDevice.Id;

            var newBrand = currentDevice.Brand + ". Change";
            var newName = currentDevice.Name;
            var newState = currentDevice.State;

            var connectionMock = Substitute.For<DbConnection>();
            var transactionMock = Substitute.For<DbTransaction>();
            var connection = connectionMock;
            var transaction = transactionMock;

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            deviceRepositoryMock
                .FetchByIdAsync(deviceId, connection, transaction, cancellationToken)
                .Returns(currentDevice);

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            databaseConnectionMock
                .CreateConnectionAndTransactionAsync(cancellationToken)
                .Returns(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var act = async () => await devicesCrudService.UpdateDeviceAsync(deviceId,
                newName: null,
                newBrand: null,
                newState: null,
                cancellationToken);

            // Assert
            await act.Should().ThrowAsync<DeviceBusinessException>();

            await deviceRepositoryMock.Received(0).FetchByIdAsync(deviceId, connection, transaction, cancellationToken);

            await deviceRepositoryMock.Received(0).UpdateAsync(currentDevice, connection, transaction, cancellationToken);

            await transactionMock.Received(0).CommitAsync(cancellationToken);
        }

        [Fact]
        public async Task UpdateDeviceAsync_When_NotFound_Throw_Exception()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            DeviceEntity? currentDevice = default;
            var deviceId = Guid.NewGuid();

            var newName = "Iphone 17 PRO MAX New";
            var newBrand = "Apple 2";
            var newState = DeviceStateEnum.InUse;

            var connectionMock = Substitute.For<DbConnection>();
            var transactionMock = Substitute.For<DbTransaction>();
            var connection = connectionMock;
            var transaction = transactionMock;

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            deviceRepositoryMock
                .FetchByIdAsync(deviceId, connection, transaction, cancellationToken)
                .Returns(currentDevice);

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            databaseConnectionMock
                .CreateConnectionAndTransactionAsync(cancellationToken)
                .Returns(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var act = async () => await devicesCrudService.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();

            await deviceRepositoryMock.Received(1).FetchByIdAsync(deviceId, connection, transaction, cancellationToken);

            await deviceRepositoryMock.Received(0).UpdateAsync(Arg.Any<DeviceEntity>(), connection, transaction, cancellationToken);

            await transactionMock.Received(0).CommitAsync(cancellationToken);
        }

        [Fact]
        public async Task UpdateDeviceAsync_When_In_Use_Throw_Exception_When_Change_Name()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            var currentDevice = new DeviceEntity
            {
                Brand = "Apple",
                Name = "Iphone 17 PRO MAX",
                State = DeviceStateEnum.InUse,
            };
            var deviceId = currentDevice.Id;

            var newName = currentDevice.Name + ". Change";
            var newBrand = currentDevice.Brand;
            var newState = currentDevice.State;

            var connectionMock = Substitute.For<DbConnection>();
            var transactionMock = Substitute.For<DbTransaction>();
            var connection = connectionMock;
            var transaction = transactionMock;

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            deviceRepositoryMock
                .FetchByIdAsync(deviceId, connection, transaction, cancellationToken)
                .Returns(currentDevice);

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            databaseConnectionMock
                .CreateConnectionAndTransactionAsync(cancellationToken)
                .Returns(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var act = async () => await devicesCrudService.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<DeviceBusinessException>();

            await deviceRepositoryMock.Received(1).FetchByIdAsync(deviceId, connection, transaction, cancellationToken);

            await deviceRepositoryMock.Received(0).UpdateAsync(currentDevice, connection, transaction, cancellationToken);

            await transactionMock.Received(0).CommitAsync(cancellationToken);
        }

        [Fact]
        public async Task UpdateDeviceAsync_When_In_Use_Throw_Exception_When_Change_Brand()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            var currentDevice = new DeviceEntity
            {
                Brand = "Apple",
                Name = "Iphone 17 PRO MAX",
                State = DeviceStateEnum.InUse,
            };
            var deviceId = currentDevice.Id;

            var newBrand = currentDevice.Brand + ". Change";
            var newName = currentDevice.Name;
            var newState = currentDevice.State;

            var connectionMock = Substitute.For<DbConnection>();
            var transactionMock = Substitute.For<DbTransaction>();
            var connection = connectionMock;
            var transaction = transactionMock;

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            deviceRepositoryMock
                .FetchByIdAsync(deviceId, connection, transaction, cancellationToken)
                .Returns(currentDevice);

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            databaseConnectionMock
                .CreateConnectionAndTransactionAsync(cancellationToken)
                .Returns(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var act = async () => await devicesCrudService.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<DeviceBusinessException>();

            await deviceRepositoryMock.Received(1).FetchByIdAsync(deviceId, connection, transaction, cancellationToken);

            await deviceRepositoryMock.Received(0).UpdateAsync(currentDevice, connection, transaction, cancellationToken);

            await transactionMock.Received(0).CommitAsync(cancellationToken);
        }

        #endregion

        #region Fetch Methods

        [Fact]
        public async Task FetchSingleDeviceAsync_Must_Call_Repository()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();
            deviceRepositoryMock
                .FetchByIdAsync(deviceId, cancellationToken)
                .Returns(new DeviceEntity
                {
                    Id = deviceId,
                    Name = "Iphone 17 PRO MAX",
                    Brand = "Apple"
                });

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var result = await devicesCrudService.FetchSingleDeviceAsync(deviceId, cancellationToken);

            // Assert
            await deviceRepositoryMock.Received(1).FetchByIdAsync(deviceId, cancellationToken);

            result.Should().NotBeNull();
            result!.Id.Should().Be(deviceId);
        }

        [Fact]
        public async Task FetchAllDevicesAsync_Must_Call_Repository()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();
            deviceRepositoryMock
                .FetchAllAsync(cancellationToken)
                .Returns(new List<DeviceEntity>
                {
                    new DeviceEntity
                    {
                        Id = deviceId,
                        Name = "Iphone 17 PRO MAX",
                        Brand = "Apple"
                    }
                });

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var result = await devicesCrudService.FetchAllDevicesAsync(cancellationToken);

            // Assert
            await deviceRepositoryMock.Received(1).FetchAllAsync(cancellationToken);

            result.Should().NotBeNull();
            result.First().Id.Should().Be(deviceId);
        }

        [Fact]
        public async Task FetchAllByBrandAsync_Must_Call_Repository()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var deviceBrand = "Apple";
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();
            deviceRepositoryMock
                .FetchAllByBrandAsync(deviceBrand, cancellationToken)
                .Returns(new List<DeviceEntity>
                {
                    new DeviceEntity
                    {
                        Id = deviceId,
                        Name = "Iphone 17 PRO MAX",
                        Brand = "Apple"
                    }
                });

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var result = await devicesCrudService.FetchAllDevicesByBrandAsync(deviceBrand, cancellationToken);

            // Assert
            await deviceRepositoryMock.Received(1).FetchAllByBrandAsync(deviceBrand, cancellationToken);

            result.Should().NotBeNull();
            result.First().Id.Should().Be(deviceId);
        }

        [Fact]
        public async Task FetchAllByStateAsync_Must_Call_Repository()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var deviceState = DeviceStateEnum.InUse;
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = Substitute.For<IDeviceRepository>();
            deviceRepositoryMock
                .FetchAllByStateAsync(deviceState, cancellationToken)
                .Returns(new List<DeviceEntity>
                {
                    new DeviceEntity
                    {
                        Id = deviceId,
                        Name = "Iphone 17 PRO MAX",
                        Brand = "Apple",
                        State = deviceState
                    }
                });

            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock,
                databaseConnectionMock
            );

            // Act
            var result = await devicesCrudService.FetchAllDevicesByStateAsync(deviceState, cancellationToken);

            // Assert
            await deviceRepositoryMock.Received(1).FetchAllByStateAsync(deviceState, cancellationToken);

            result.Should().NotBeNull();
            result.First().Id.Should().Be(deviceId);
        }

        #endregion
    }
}
