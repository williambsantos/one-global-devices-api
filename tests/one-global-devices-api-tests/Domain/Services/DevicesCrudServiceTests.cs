using Microsoft.AspNetCore.Localization;
using Moq;
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

            var deviceRepositoryMock = new Mock<IDeviceRepository>();
            var databaseConnectionMock = new Mock<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            DeviceEntity deviceCreated = await devicesCrudService.CreateNewDeviceAsync(
                name, brand,
                cancellationToken
            );

            // Assert
            Assert.NotNull(deviceCreated);
            Assert.Equal(deviceCreated.Name, name);
            Assert.Equal(deviceCreated.Brand, brand);

            deviceRepositoryMock.Verify(
                expression: repo => repo.SaveAsync(It.IsAny<DeviceEntity>(), cancellationToken),
                times: Times.Once
            );
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteSingleDeviceAsync_Must_Fetch_And_Delete()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var connectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var connection = connectionMock.Object;
            var transaction = transactionMock.Object;

            var deviceRepositoryMock = new Mock<IDeviceRepository>();

            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken))
                .ReturnsAsync(new DeviceEntity
                {
                    Id = deviceId,
                    Name = "Iphone 17 PRO MAX",
                    Brand = "Apple"
                });

            var databaseConnectionMock = new Mock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(con => con.CreateConnectionAndTransactionAsync(cancellationToken))
                .ReturnsAsync(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            await devicesCrudService.DeleteSingleDeviceAsync(deviceId, cancellationToken);

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Once
            );

            deviceRepositoryMock.Verify(
                expression: repo => repo.DeleteAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Once
            );

            transactionMock.Verify(
                expression: tran => tran.CommitAsync(cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task DeleteSingleDeviceAsync_When_NotFound_Throw_Exception()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var connectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var connection = connectionMock.Object;
            var transaction = transactionMock.Object;

            var deviceRepositoryMock = new Mock<IDeviceRepository>();

            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken))
                .ReturnsAsync(new DeviceEntity
                {
                    Id = deviceId,
                    Name = "Iphone 17 PRO MAX",
                    Brand = "Apple",
                    State = DeviceStateEnum.InUse
                });

            var databaseConnectionMock = new Mock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(con => con.CreateConnectionAndTransactionAsync(cancellationToken))
                .ReturnsAsync(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            await Assert.ThrowsAsync<DeviceBusinessException>(async () =>
                await devicesCrudService.DeleteSingleDeviceAsync(deviceId, cancellationToken)
            );

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Once
            );

            deviceRepositoryMock.Verify(
                expression: repo => repo.DeleteAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Never
            );

            transactionMock.Verify(
                expression: tran => tran.CommitAsync(cancellationToken),
                times: Times.Never
            );
        }

        [Fact]
        public async Task DeleteSingleDeviceAsync_When_Fetch_In_Use_Throw_Exception()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var connectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var connection = connectionMock.Object;
            var transaction = transactionMock.Object;

            var deviceRepositoryMock = new Mock<IDeviceRepository>();

            DeviceEntity? deviceNotFound = null;

            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken))
                .ReturnsAsync(deviceNotFound);

            var databaseConnectionMock = new Mock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(con => con.CreateConnectionAndTransactionAsync(cancellationToken))
                .ReturnsAsync(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await devicesCrudService.DeleteSingleDeviceAsync(deviceId, cancellationToken)
            );

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Once
            );

            deviceRepositoryMock.Verify(
                expression: repo => repo.DeleteAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Never
            );

            transactionMock.Verify(
                expression: tran => tran.CommitAsync(cancellationToken),
                times: Times.Never
            );
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

            var connectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var connection = connectionMock.Object;
            var transaction = transactionMock.Object;

            var deviceRepositoryMock = new Mock<IDeviceRepository>();

            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken))
                .ReturnsAsync(currentDevice);

            var databaseConnectionMock = new Mock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(con => con.CreateConnectionAndTransactionAsync(cancellationToken))
                .ReturnsAsync(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            await devicesCrudService.UpdateDeviceAsync(
                deviceId, newName, newBrand, newState, cancellationToken
            );

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Once
            );

            deviceRepositoryMock.Verify(
                expression: repo => repo.UpdateAsync(currentDevice, connection, transaction, cancellationToken),
                times: Times.Once
            );

            transactionMock.Verify(
                expression: tran => tran.CommitAsync(cancellationToken),
                times: Times.Once
            );
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

            var connectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var connection = connectionMock.Object;
            var transaction = transactionMock.Object;

            var deviceRepositoryMock = new Mock<IDeviceRepository>();

            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken))
                .ReturnsAsync(currentDevice);

            var databaseConnectionMock = new Mock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(con => con.CreateConnectionAndTransactionAsync(cancellationToken))
                .ReturnsAsync(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            await devicesCrudService.UpdateDeviceAsync(
                deviceId, newName, newBrand, newState, cancellationToken
            );

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Once
            );

            deviceRepositoryMock.Verify(
                expression: repo => repo.UpdateAsync(currentDevice, connection, transaction, cancellationToken),
                times: Times.Never
            );

            transactionMock.Verify(
                expression: tran => tran.CommitAsync(cancellationToken),
                times: Times.Never
            );
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

            var connectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var connection = connectionMock.Object;
            var transaction = transactionMock.Object;

            var deviceRepositoryMock = new Mock<IDeviceRepository>();

            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken))
                .ReturnsAsync(currentDevice);

            var databaseConnectionMock = new Mock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(con => con.CreateConnectionAndTransactionAsync(cancellationToken))
                .ReturnsAsync(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            await Assert.ThrowsAsync<DeviceBusinessException>(async () =>
                await devicesCrudService.UpdateDeviceAsync(deviceId,
                newName: null,
                newBrand: null,
                newState: null,
                cancellationToken)
            );

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Never
            );

            deviceRepositoryMock.Verify(
                expression: repo => repo.UpdateAsync(currentDevice, connection, transaction, cancellationToken),
                times: Times.Never
            );

            transactionMock.Verify(
                expression: tran => tran.CommitAsync(cancellationToken),
                times: Times.Never
            );
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

            var connectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var connection = connectionMock.Object;
            var transaction = transactionMock.Object;

            var deviceRepositoryMock = new Mock<IDeviceRepository>();

            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken))
                .ReturnsAsync(currentDevice);

            var databaseConnectionMock = new Mock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(con => con.CreateConnectionAndTransactionAsync(cancellationToken))
                .ReturnsAsync(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await devicesCrudService.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken)
            );

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Once
            );

            deviceRepositoryMock.Verify(
                expression: repo => repo.UpdateAsync(It.IsAny<DeviceEntity>(), connection, transaction, cancellationToken),
                times: Times.Never
            );

            transactionMock.Verify(
                expression: tran => tran.CommitAsync(cancellationToken),
                times: Times.Never
            );
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

            var connectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var connection = connectionMock.Object;
            var transaction = transactionMock.Object;

            var deviceRepositoryMock = new Mock<IDeviceRepository>();

            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken))
                .ReturnsAsync(currentDevice);

            var databaseConnectionMock = new Mock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(con => con.CreateConnectionAndTransactionAsync(cancellationToken))
                .ReturnsAsync(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            await Assert.ThrowsAsync<DeviceBusinessException>(async () =>
                await devicesCrudService.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken)
            );

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Once
            );

            deviceRepositoryMock.Verify(
                expression: repo => repo.UpdateAsync(currentDevice, connection, transaction, cancellationToken),
                times: Times.Never
            );

            transactionMock.Verify(
                expression: tran => tran.CommitAsync(cancellationToken),
                times: Times.Never
            );
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

            var connectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var connection = connectionMock.Object;
            var transaction = transactionMock.Object;

            var deviceRepositoryMock = new Mock<IDeviceRepository>();

            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken))
                .ReturnsAsync(currentDevice);

            var databaseConnectionMock = new Mock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(con => con.CreateConnectionAndTransactionAsync(cancellationToken))
                .ReturnsAsync(new DatabaseWork(connection, transaction));

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            await Assert.ThrowsAsync<DeviceBusinessException>(async () =>
                await devicesCrudService.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken)
            );

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, connection, transaction, cancellationToken),
                times: Times.Once
            );

            deviceRepositoryMock.Verify(
                expression: repo => repo.UpdateAsync(currentDevice, connection, transaction, cancellationToken),
                times: Times.Never
            );

            transactionMock.Verify(
                expression: tran => tran.CommitAsync(cancellationToken),
                times: Times.Never
            );
        }

        #endregion

        #region Fetch Methods

        [Fact]
        public async Task FetchSingleDeviceAsync_Must_Call_Repository()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = new Mock<IDeviceRepository>();
            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, cancellationToken))
                .ReturnsAsync(new DeviceEntity
                {
                    Id = deviceId,
                    Name = "Iphone 17 PRO MAX",
                    Brand = "Apple"
                });

            var databaseConnectionMock = new Mock<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            var result = await devicesCrudService.FetchSingleDeviceAsync(deviceId, cancellationToken);

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchByIdAsync(deviceId, cancellationToken),
                times: Times.Once
            );

            Assert.NotNull(result);
            Assert.Equal(deviceId, result.Id);
        }

        [Fact]
        public async Task FetchAllDevicesAsync_Must_Call_Repository()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = new Mock<IDeviceRepository>();
            deviceRepositoryMock
                .Setup(repo => repo.FetchAllAsync(cancellationToken))
                .ReturnsAsync(new List<DeviceEntity>
                {
                    new DeviceEntity
                    {
                        Id = deviceId,
                        Name = "Iphone 17 PRO MAX",
                        Brand = "Apple"
                    }
                });

            var databaseConnectionMock = new Mock<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            var result = await devicesCrudService.FetchAllDevicesAsync(cancellationToken);

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchAllAsync(cancellationToken),
                times: Times.Once
            );

            Assert.NotNull(result);
            Assert.Equal(deviceId, result.First().Id);
        }

        [Fact]
        public async Task FetchAllByBrandAsync_Must_Call_Repository()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var deviceBrand = "Apple";
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = new Mock<IDeviceRepository>();
            deviceRepositoryMock
                .Setup(repo => repo.FetchAllByBrandAsync(deviceBrand, cancellationToken))
                .ReturnsAsync(new List<DeviceEntity>
                {
                    new DeviceEntity
                    {
                        Id = deviceId,
                        Name = "Iphone 17 PRO MAX",
                        Brand = "Apple"
                    }
                });

            var databaseConnectionMock = new Mock<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            var result = await devicesCrudService.FetchAllDevicesByBrandAsync(deviceBrand, cancellationToken);

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchAllByBrandAsync(deviceBrand, cancellationToken),
                times: Times.Once
            );

            Assert.NotNull(result);
            Assert.Equal(deviceId, result.First().Id);
        }

        [Fact]
        public async Task FetchAllByStateAsync_Must_Call_Repository()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var deviceState = DeviceStateEnum.InUse;
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = new Mock<IDeviceRepository>();
            deviceRepositoryMock
                .Setup(repo => repo.FetchAllByStateAsync(deviceState, cancellationToken))
                .ReturnsAsync(new List<DeviceEntity>
                {
                    new DeviceEntity
                    {
                        Id = deviceId,
                        Name = "Iphone 17 PRO MAX",
                        Brand = "Apple",
                        State = deviceState
                    }
                });

            var databaseConnectionMock = new Mock<IDatabaseConnection>();

            var devicesCrudService = new DevicesCrudService(
                deviceRepositoryMock.Object,
                databaseConnectionMock.Object
            );

            // Act
            var result = await devicesCrudService.FetchAllDevicesByStateAsync(deviceState, cancellationToken);

            // Assert
            deviceRepositoryMock.Verify(
                expression: repo => repo.FetchAllByStateAsync(deviceState, cancellationToken),
                times: Times.Once
            );

            Assert.NotNull(result);
            Assert.Equal(deviceId, result.First().Id);
        }

        #endregion
    }
}
