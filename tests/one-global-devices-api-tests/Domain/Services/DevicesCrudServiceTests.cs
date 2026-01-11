using Moq;
using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Repositories;
using OneGlobalDevicesApi.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace one_global_devices_api_tests.Domain.Services
{
    public class DevicesCrudServiceTests
    {
        [Fact]
        public async Task FetchSingleDeviceAsync_Must_Call_Repository()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var deviceRepositoryMock = new Mock<IDeviceRepository>();
            deviceRepositoryMock
                .Setup(repo => repo.FetchByIdAsync(deviceId, cancellationToken))
                .ReturnsAsync(new DeviceEntity { 
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

            // Act

            // Assert
        }

        [Fact]
        public async Task FetchAllByBrandAsync_Must_Call_Repository()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public async Task FetchAllByStateAsync_Must_Call_Repository()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
