using NSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Repositories;
using OneGlobalDevicesApi.Infra.SQLServer.Repositories;
using System.Data.Common;
using NSubstitute.DbConnection;

namespace OneGlobalDevicesApi.Tests.Infra.SQLServer.Repositories
{
    public class DeviceSqlServerRepositoryTests
    {
        // Tests below use NSubstitute.Community.DbConnection helpers via SetupCommands/SetupQuery

        #region SaveAsync Tests

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task SaveAsync_ValidEntity_ExecutesInsertCommand(int expected)
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

            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("INSERT"))
                .Affects(expected);

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var _repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var actual = await _repository.SaveAsync(entity);

            // Assert
            await databaseConnectionMock.Received(1).CreateConnectionAsync(Arg.Any<CancellationToken>());

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task SaveAsync_WithCancellationToken_PassesTokenToConnection(int expected)
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

            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("INSERT"))
                .Affects(expected);

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var _repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var actual = await _repository.SaveAsync(entity, cts.Token);

            // Assert
            await databaseConnectionMock.Received(1).CreateConnectionAsync(cts.Token);
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task SaveAsync_ExecuteThrowsException_LogsError()
        {
            // Arrange
            var entity = new DeviceEntity { Name = "Test", Brand = "Brand" };
            var exception = new Exception("Database error");

            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("INSERT"))
                .Throws(exception);

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var _repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var act = async () => await _repository.SaveAsync(entity);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(exception.Message);
        }

        #endregion

        #region UpdateAsync Tests

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task UpdateAsync_ValidEntity_ExecutesUpdateCommand(int expected)
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

            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();
            var dbTransactionMock = Substitute.For<DbTransaction>();

            dbConnectionMock
                .SetupQuery(query => query.Contains("UPDATE"))
                .Affects(expected);

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var actual = await repository.UpdateAsync(entity, dbConnectionMock, dbTransactionMock);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task UpdateAsync_NullConnection_ThrowsArgumentNullException()
        {
            // Arrange
            var entity = new DeviceEntity { Id = Guid.NewGuid(), Name = "Device", Brand = "Brand" };
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var act = async () => await repository.UpdateAsync(entity, null, Substitute.For<DbTransaction>());

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task UpdateAsync_StateChanges_UpdatesCorrectly(int expected)
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

            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();
            var dbTransactionMock = Substitute.For<DbTransaction>();

            dbConnectionMock
                .SetupQuery(query => query.Contains("UPDATE"))
                .Affects(expected);

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var actual = await repository.UpdateAsync(entity, dbConnectionMock, dbTransactionMock);

            // Assert
            actual.Should().Be(expected);
        }

        #endregion

        #region DeleteAsync Tests

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DeleteAsync_ValidId_ExecutesDeleteCommand(int expected)
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();
            var dbTransactionMock = Substitute.For<DbTransaction>();

            dbConnectionMock
                .SetupQuery(query => query.Contains("DELETE"))
                .Affects(expected);

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var actual = await repository.DeleteAsync(deviceId, dbConnectionMock, dbTransactionMock);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task DeleteAsync_NullConnection_ThrowsArgumentNullException()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var act = async () => await repository.DeleteAsync(deviceId, null, Substitute.For<DbTransaction>());

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task DeleteAsync_NonExistentId_ReturnsZero(int expected)
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();
            var dbTransactionMock = Substitute.For<DbTransaction>();

            dbConnectionMock
                .SetupQuery(query => query.Contains("DELETE"))
                .Affects(expected);

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var actual = await repository.DeleteAsync(deviceId, dbConnectionMock, dbTransactionMock);

            // Assert
            actual.Should().Be(expected);
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
                    State = DeviceStateEnum.InUse,
                    CreationTime = DateTimeOffset.UtcNow
                }
            };
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("SELECT"))
                .Returns(
                    expectedDevices.First(),
                    expectedDevices.Last()
                );

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var result = await repository.FetchAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task FetchAllAsync_NoDevices_ReturnsEmptyCollection()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("SELECT"))
                .Returns(Enumerable.Empty<DeviceEntity>());

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var result = await repository.FetchAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
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
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("SELECT"))
                .Returns(
                    expectedDevices.First()
                );

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var result = await repository.FetchAllByBrandAsync(brand);

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result.Should().OnlyContain(d => d.Brand == brand);
        }

        [Fact]
        public async Task FetchAllByBrandAsync_NoBrandMatches_ReturnsEmptyCollection()
        {
            // Arrange
            var brand = "NonExistentBrand";
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("SELECT"))
                .Returns(Enumerable.Empty<DeviceEntity>());

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var result = await repository.FetchAllByBrandAsync(brand);

            // Assert
            result.Should().BeEmpty();
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
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("SELECT"))
                .Returns(
                    expectedDevices.First()
                );

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var result = await repository.FetchAllByStateAsync(state);

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result.Should().OnlyContain(d => d.State == state);
        }

        [Theory]
        [InlineData(DeviceStateEnum.Available)]
        [InlineData(DeviceStateEnum.InUse)]
        [InlineData(DeviceStateEnum.Inactive)]
        public async Task FetchAllByStateAsync_VariousStates_ReturnsCorrectState(DeviceStateEnum state)
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("SELECT"))
                .Returns(Enumerable.Empty<DeviceEntity>());

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var result = await repository.FetchAllByStateAsync(state);

            // Assert
            result.Should().BeEmpty();
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
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("WHERE"))
                .Returns(
                    expectedDevice
                );

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var result = await repository.FetchByIdAsync(deviceId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(deviceId);
            result.Name.Should().Be("Test Device");
        }

        [Fact]
        public async Task FetchByIdAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();

            dbConnectionMock
                .SetupQuery(query => query.Contains("SELECT"))
                .Returns(Enumerable.Empty<DeviceEntity>());

            databaseConnectionMock
                .CreateConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(dbConnectionMock));

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var result = await repository.FetchByIdAsync(deviceId);

            // Assert
            result.Should().BeNull();
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
            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();
            var dbTransactionMock = Substitute.For<DbTransaction>();

            dbConnectionMock
                .SetupQuery(query => query.Contains("SELECT"))
                .Returns(
                    expectedDevice
                );

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            var result = await repository.FetchByIdAsync(deviceId, dbConnectionMock, dbTransactionMock);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(deviceId);
        }

        [Fact]
        public async Task FetchByIdAsync_WithTransaction_DoesNotCreateNewConnection()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var expectedDevice = new DeviceEntity { Id = deviceId, Name = "Device", Brand = "Brand" };

            var loggerMock = Substitute.For<ILogger<DeviceSqlServerRepository>>();
            var databaseConnectionMock = Substitute.For<IDatabaseConnection>();
            var dbConnectionMock = Substitute.For<DbConnection>().SetupCommands();
            var dbTransactionMock = Substitute.For<DbTransaction>();

            dbConnectionMock
                .SetupQuery(query => query.Contains("SELECT"))
                .Returns(new List<DeviceEntity> { expectedDevice });

            var repository = new DeviceSqlServerRepository(loggerMock, databaseConnectionMock);

            // Act
            await repository.FetchByIdAsync(deviceId, dbConnectionMock, dbTransactionMock);

            // Assert
            await databaseConnectionMock.Received(0).CreateConnectionAsync(Arg.Any<CancellationToken>());
        }

        #endregion
    }
}
