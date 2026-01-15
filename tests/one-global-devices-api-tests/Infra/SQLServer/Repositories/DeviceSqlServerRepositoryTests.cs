using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Repositories;
using OneGlobalDevicesApi.Infra.SQLServer.Connections;
using OneGlobalDevicesApi.Infra.SQLServer.Repositories;
using System.Data.Common;
using Dapper;

namespace OneGlobalDevicesApi.Tests.Infra.SQLServer.Repositories
{
    public class DeviceSqlServerRepositoryTests
    {
        private readonly Mock<ILogger<DeviceSqlServerRepository>> _loggerMock;
        private readonly Mock<IDatabaseConnection> _databaseConnectionMock;
        private readonly Mock<DbConnection> _dbConnectionMock;
        private readonly Mock<DbTransaction> _dbTransactionMock;
        private readonly DeviceSqlServerRepository _repository;

        public DeviceSqlServerRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<DeviceSqlServerRepository>>();
            _databaseConnectionMock = new Mock<IDatabaseConnection>();
            _dbConnectionMock = new Mock<DbConnection>();
            _dbTransactionMock = new Mock<DbTransaction>();
            _repository = new DeviceSqlServerRepository(_loggerMock.Object, _databaseConnectionMock.Object);
        }

        #region SaveAsync Tests

        [Fact]
        public async Task SaveAsync_ValidEntity_ExecutesInsertCommand()
        {
            // Arrange
            var entity = new DeviceEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test Device",
                Brand = "Test Brand",
                State = DeviceStateEnum.Available,
                CreationTime = DateTimeOffset.UtcNow
            };

            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1);

            // Act
            await _repository.SaveAsync(entity);

            // Assert
            _databaseConnectionMock.Verify(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _dbConnectionMock.Verify(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_WithCancellationToken_PassesTokenToConnection()
        {
            // Arrange
            var entity = new DeviceEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Brand = "Brand",
                State = DeviceStateEnum.Available,
                CreationTime = DateTimeOffset.UtcNow
            };
            var cts = new CancellationTokenSource();

            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(cts.Token))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1);

            // Act
            await _repository.SaveAsync(entity, cts.Token);

            // Assert
            _databaseConnectionMock.Verify(x => x.CreateConnectionAsync(cts.Token), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_ExecuteThrowsException_LogsError()
        {
            // Arrange
            var entity = new DeviceEntity { Name = "Test",  Brand = "Brand" };
            var exception = new Exception("Database error");

            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()))
                .ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _repository.SaveAsync(entity));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ValidEntity_ExecutesUpdateCommand()
        {
            // Arrange
            var entity = new DeviceEntity
            {
                Id = Guid.NewGuid(),
                Name = "Updated Device",
                Brand = "Updated Brand",
                State = DeviceStateEnum.Inactive,
                CreationTime = DateTimeOffset.UtcNow
            };

            _dbConnectionMock
                .Setup(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1);

            // Act
            await _repository.UpdateAsync(entity, _dbConnectionMock.Object, _dbTransactionMock.Object);

            // Assert
            _dbConnectionMock.Verify(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NullConnection_ThrowsArgumentNullException()
        {
            // Arrange
            var entity = new DeviceEntity { Id = Guid.NewGuid(), Name = "Device", Brand = "Brand" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.UpdateAsync(entity, null, _dbTransactionMock.Object));
        }

        [Fact]
        public async Task UpdateAsync_StateChanges_UpdatesCorrectly()
        {
            // Arrange
            var entity = new DeviceEntity
            {
                Id = Guid.NewGuid(),
                Name = "Device",
                Brand = "Brand",
                State = DeviceStateEnum.InUse,
                CreationTime = DateTimeOffset.UtcNow
            };

            _dbConnectionMock
                .Setup(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1);

            // Act
            await _repository.UpdateAsync(entity, _dbConnectionMock.Object, _dbTransactionMock.Object);

            // Assert
            _dbConnectionMock.Verify(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ValidId_ExecutesDeleteCommand()
        {
            // Arrange
            var deviceId = Guid.NewGuid();

            _dbConnectionMock
                .Setup(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1);

            // Act
            await _repository.DeleteAsync(deviceId, _dbConnectionMock.Object, _dbTransactionMock.Object);

            // Assert
            _dbConnectionMock.Verify(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NullConnection_ThrowsArgumentNullException()
        {
            // Arrange
            var deviceId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.DeleteAsync(deviceId, null, _dbTransactionMock.Object));
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ReturnsZero()
        {
            // Arrange
            var deviceId = Guid.NewGuid();

            _dbConnectionMock
                .Setup(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(0);

            // Act
            await _repository.DeleteAsync(deviceId, _dbConnectionMock.Object, _dbTransactionMock.Object);

            // Assert
            _dbConnectionMock.Verify(x => x.ExecuteAsync(It.IsAny<CommandDefinition>()), Times.Once);
        }

        #endregion

        #region FetchAllAsync Tests

        [Fact]
        public async Task FetchAllAsync_ReturnsAllDevices()
        {
            // Arrange
            var expectedDevices = new List<DeviceEntity>
            {
                new DeviceEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Device1",
                    Brand = "Brand1",
                    State = DeviceStateEnum.Available,
                    CreationTime = DateTimeOffset.UtcNow
                },
                new DeviceEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Device2",
                    Brand = "Brand2",
                    State = DeviceStateEnum.Inactive,
                    CreationTime = DateTimeOffset.UtcNow
                }
            };

            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(expectedDevices.AsEnumerable());

            // Act
            var result = await _repository.FetchAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task FetchAllAsync_NoDevices_ReturnsEmptyCollection()
        {
            // Arrange
            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(Enumerable.Empty<DeviceEntity>());

            // Act
            var result = await _repository.FetchAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region FetchAllByBrandAsync Tests

        [Fact]
        public async Task FetchAllByBrandAsync_ValidBrand_ReturnsMatchingDevices()
        {
            // Arrange
            var brand = "TestBrand";
            var expectedDevices = new List<DeviceEntity>
            {
                new DeviceEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Device1",
                    Brand = brand,
                    State = DeviceStateEnum.Available,
                    CreationTime = DateTimeOffset.UtcNow
                }
            };

            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(expectedDevices.AsEnumerable());

            // Act
            var result = await _repository.FetchAllByBrandAsync(brand);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, d => Assert.Equal(brand, d.Brand));
        }

        [Fact]
        public async Task FetchAllByBrandAsync_NoBrandMatches_ReturnsEmptyCollection()
        {
            // Arrange
            var brand = "NonExistentBrand";

            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(Enumerable.Empty<DeviceEntity>());

            // Act
            var result = await _repository.FetchAllByBrandAsync(brand);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region FetchAllByStateAsync Tests

        [Fact]
        public async Task FetchAllByStateAsync_ValidState_ReturnsMatchingDevices()
        {
            // Arrange
            var state = DeviceStateEnum.Available;
            var expectedDevices = new List<DeviceEntity>
            {
                new DeviceEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Device1",
                    Brand = "Brand1",
                    State = state,
                    CreationTime = DateTimeOffset.UtcNow
                }
            };

            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(expectedDevices.AsEnumerable());

            // Act
            var result = await _repository.FetchAllByStateAsync(state);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, d => Assert.Equal(state, d.State));
        }

        [Theory]
        [InlineData(DeviceStateEnum.Available)]
        [InlineData(DeviceStateEnum.InUse)]
        [InlineData(DeviceStateEnum.Inactive)]
        public async Task FetchAllByStateAsync_VariousStates_ReturnsCorrectState(DeviceStateEnum state)
        {
            // Arrange
            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(Enumerable.Empty<DeviceEntity>());

            // Act
            var result = await _repository.FetchAllByStateAsync(state);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region FetchByIdAsync Tests

        [Fact]
        public async Task FetchByIdAsync_ValidId_ReturnsDevice()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var expectedDevice = new DeviceEntity
            {
                Id = deviceId,
                Name = "Test Device",
                Brand = "Test Brand",
                State = DeviceStateEnum.Available,
                CreationTime = DateTimeOffset.UtcNow
            };

            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(new List<DeviceEntity> { expectedDevice }.AsEnumerable());

            // Act
            var result = await _repository.FetchByIdAsync(deviceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(deviceId, result.Id);
            Assert.Equal("Test Device", result.Name);
        }

        [Fact]
        public async Task FetchByIdAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            var deviceId = Guid.NewGuid();

            _databaseConnectionMock
                .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbConnectionMock.Object);

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(Enumerable.Empty<DeviceEntity>());

            // Act
            var result = await _repository.FetchByIdAsync(deviceId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FetchByIdAsync_WithConnectionAndTransaction_ReturnsDevice()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var expectedDevice = new DeviceEntity
            {
                Id = deviceId,
                Name = "Test",
                Brand = "Brand",
                State = DeviceStateEnum.Available,
                CreationTime = DateTimeOffset.UtcNow
            };

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(new List<DeviceEntity> { expectedDevice }.AsEnumerable());

            // Act
            var result = await _repository.FetchByIdAsync(deviceId, _dbConnectionMock.Object, _dbTransactionMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(deviceId, result.Id);
        }

        [Fact]
        public async Task FetchByIdAsync_WithTransaction_DoesNotCreateNewConnection()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var expectedDevice = new DeviceEntity { Id = deviceId, Name = "Device", Brand = "Brand" };

            _dbConnectionMock
                .Setup(x => x.QueryAsync<DeviceEntity>(It.IsAny<CommandDefinition>()))
                .ReturnsAsync(new List<DeviceEntity> { expectedDevice }.AsEnumerable());

            // Act
            await _repository.FetchByIdAsync(deviceId, _dbConnectionMock.Object, _dbTransactionMock.Object);

            // Assert
            _databaseConnectionMock.Verify(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}
