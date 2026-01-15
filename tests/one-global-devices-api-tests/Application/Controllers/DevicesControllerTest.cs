using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OneGlobalDevicesApi.Application.Controllers;
using OneGlobalDevicesApi.Application.DTOs;
using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Services;

namespace OneGlobalDevicesApiTests.Application.Controllers
{
    public class DevicesControllerTest
    {
        #region Common Asserts

        private void AssertActionResultDeviceResponseDto(IEnumerable<DeviceEntity> expectedList, ActionResult<IEnumerable<DeviceResponseDto>> actionResponse)
        {
            Assert.NotNull(actionResponse);

            var okResult = Assert.IsType<OkObjectResult>(actionResponse.Result);
            Assert.NotNull(okResult);

            var actual = okResult.Value as IEnumerable<DeviceResponseDto> ??
                throw new InvalidOperationException("Expected a DeviceResponseDto");

            for (int i = 0; i < expectedList.Count(); i++)
            {
                AssertDeviceEntity(expectedList.ElementAt(i), actual.ElementAt(i));
            }
        }

        private void AssertActionResultDeviceResponseDto(DeviceEntity expected, ActionResult<DeviceResponseDto> actionResponse)
        {
            Assert.NotNull(actionResponse);

            var okResult = Assert.IsType<OkObjectResult>(actionResponse.Result);
            Assert.NotNull(okResult);

            var actual = okResult.Value as DeviceResponseDto ??
                throw new InvalidOperationException("Expected a DeviceResponseDto");

            AssertDeviceEntity(expected, actual);
        }

        private void AssertDeviceEntity(DeviceEntity expected, DeviceResponseDto actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Brand, actual.Brand);
            Assert.Equal(expected.State.ToString(), actual.State);
        }

        private void AssertBadRequestObjectResult<T>(Exception exception, ActionResult<T> actionResponse)
        {
            Assert.NotNull(actionResponse);

            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResponse);
            Assert.Equal(exception.Message, badRequest.Value);
        }        

        #endregion

        #region Save

        [Fact]
        public async Task CreateNewDeviceAsync_Must_Ok()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var name = "IPHONE 17 PRO MAX";
            var brand = "Apple";
            var cancellationToken = new CancellationToken();

            var dto = new DeviceCreateRequestDTO
            {
                Brand = brand,
                Name = name
            };

            var deviceCreated = new DeviceEntity
            {
                Brand = brand,
                Name = name,
                State = DeviceStateEnum.Available
            };

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(s => s.CreateNewDeviceAsync(name, brand, cancellationToken))
                .ReturnsAsync(deviceCreated);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.CreateNewDeviceAsync(
                deviceServiceMock.Object,
                dto,
                cancellationToken
            );

            // Assert
            Assert.NotNull(actionResponse);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResponse.Result);
            Assert.NotNull(createdResult.Value);
            Assert.Equal(nameof(controller.FetchSingleDevice), createdResult.ActionName);

            var actual = createdResult.Value as DeviceResponseDto ?? throw new InvalidOperationException("value is not a DeviceResponseDto");
            AssertDeviceEntity(deviceCreated, actual);

            deviceServiceMock.Verify(
                expression: repo => repo.CreateNewDeviceAsync(name, brand, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task CreateNewDeviceAsync_When_ERROR_Log_And_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var name = "IPHONE 17 PRO MAX";
            var brand = "Apple";
            var cancellationToken = new CancellationToken();

            var dto = new DeviceCreateRequestDTO
            {
                Brand = brand,
                Name = name
            };

            var deviceCreated = new DeviceEntity
            {
                Brand = brand,
                Name = name,
                State = DeviceStateEnum.Available
            };

            var exception = new Exception("Service error");

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(s => s.CreateNewDeviceAsync(name, brand, cancellationToken))
                .ThrowsAsync(exception);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.CreateNewDeviceAsync(
                deviceServiceMock.Object,
                dto,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            deviceServiceMock.Verify(
                expression: s => s.CreateNewDeviceAsync(name, brand, cancellationToken),
                times: Times.Once
            );
        }

        #endregion

        #region FullyUpdate

        [Fact]
        public async Task FullyUpdateDeviceAsync_Must_Ok()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var newName = "IPHONE 17 PRO MAX";
            var newBrand = "Apple";
            var newState = DeviceStateEnum.InUse;
            var cancellationToken = new CancellationToken();

            var dto = new DeviceFullyUpdateRequestDTO
            {
                NewBrand = newBrand,
                NewName = newName,
                NewState = newState
            };

            var device = new DeviceEntity
            {
                Brand = newBrand,
                Name = newName,
                State = DeviceStateEnum.Available
            };

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(s => s.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken))
                .ReturnsAsync(device);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FullyUpdateDeviceAsync(
                deviceServiceMock.Object,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(device, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task FullyUpdateDeviceAsync_When_ERROR_Log_And_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var newName = "IPHONE 17 PRO MAX";
            var newBrand = "Apple";
            var newState = DeviceStateEnum.InUse;
            var cancellationToken = new CancellationToken();

            var dto = new DeviceFullyUpdateRequestDTO
            {
                NewBrand = newBrand,
                NewName = newName,
                NewState = newState
            };

            var device = new DeviceEntity
            {
                Brand = newBrand,
                Name = newName,
                State = DeviceStateEnum.Available
            };

            var exception = new Exception("some exception");

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(s => s.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken))
                .ThrowsAsync(exception);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FullyUpdateDeviceAsync(
                deviceServiceMock.Object,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken),
                times: Times.Once
            );
        }

        #endregion

        #region PartiallyUpdateDeviceAsync

        [Fact]
        public async Task PartiallyUpdateDeviceAsync_Must_Ok()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var newName = "IPHONE 17 PRO MAX";
            var newBrand = "Apple";
            var newState = DeviceStateEnum.InUse;
            var cancellationToken = new CancellationToken();

            var dto = new DevicePartiallyUpdateRequestDTO
            {
                NewBrand = newBrand,
                NewName = newName,
                NewState = newState
            };

            var device = new DeviceEntity
            {
                Brand = newBrand,
                Name = newName,
                State = DeviceStateEnum.Available
            };

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(s => s.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken))
                .ReturnsAsync(device);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.PartiallyUpdateDeviceAsync(
                deviceServiceMock.Object,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(device, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task PartiallyUpdateDeviceAsync_When_ERROR_Log_And_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var newName = "IPHONE 17 PRO MAX";
            var newBrand = "Apple";
            var newState = DeviceStateEnum.InUse;
            var cancellationToken = new CancellationToken();

            var dto = new DevicePartiallyUpdateRequestDTO
            {
                NewBrand = newBrand,
                NewName = newName,
                NewState = newState
            };

            var device = new DeviceEntity
            {
                Brand = newBrand,
                Name = newName,
                State = DeviceStateEnum.Available
            };

            var exception = new Exception("some exception");

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(s => s.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken))
                .ThrowsAsync(exception);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.PartiallyUpdateDeviceAsync(
                deviceServiceMock.Object,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken),
                times: Times.Once
            );
        }

        #endregion

        #region DeleteSingleDevice

        [Fact]
        public async Task DeleteSingleDevice_Must_Ok()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.DeleteSingleDevice(
                deviceServiceMock.Object,
                deviceId,
                cancellationToken
            );

            // Assert
            Assert.NotNull(actionResponse);
            Assert.True(actionResponse.Result is NoContentResult);

            deviceServiceMock.Verify(
                expression: repo => repo.DeleteSingleDeviceAsync(deviceId, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task DeleteSingleDevice_When_KeyNotFoundException_Log_And_NotFound()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var exception = new KeyNotFoundException("some exception");

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(s => s.DeleteSingleDeviceAsync(deviceId, cancellationToken))
                .ThrowsAsync(exception);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.DeleteSingleDevice(
                deviceServiceMock.Object,
                deviceId,
                cancellationToken
            );

            // Assert
            Assert.NotNull(actionResponse);
            Assert.True(actionResponse.Result is NotFoundResult);

            deviceServiceMock.Verify(
                expression: repo => repo.DeleteSingleDeviceAsync(deviceId, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task DeleteSingleDevice_When_ERROR_Log_And_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var exception = new Exception("some exception");

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(s => s.DeleteSingleDeviceAsync(deviceId, cancellationToken))
                .ThrowsAsync(exception);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.DeleteSingleDevice(
                deviceServiceMock.Object,
                deviceId,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.DeleteSingleDeviceAsync(deviceId, cancellationToken),
                times: Times.Once
            );
        }

        #endregion

        #region FetchSingleDevice

        [Fact]
        public async Task FetchSingleDevice_Must_Ok()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var device = new DeviceEntity
            {
                Brand = "Apple",
                Name = "IPHONE 17 PRO MAX",
                State = DeviceStateEnum.Available
            };

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchSingleDeviceAsync(deviceId, cancellationToken))
                .ReturnsAsync(device);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FetchSingleDevice(
                deviceServiceMock.Object,
                deviceId,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(device, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchSingleDeviceAsync(deviceId, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task FetchSingleDevice_When_Return_Null_Call_NotFound()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            DeviceEntity? device = null;

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchSingleDeviceAsync(deviceId, cancellationToken))
                .ReturnsAsync(device);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FetchSingleDevice(
                deviceServiceMock.Object,
                deviceId,
                cancellationToken
            );

            // Assert
            Assert.NotNull(actionResponse);
            Assert.True(actionResponse.Result is NotFoundResult);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchSingleDeviceAsync(deviceId, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task FetchSingleDevice_When_Exception_KeyNotFound_Call_NotFound()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchSingleDeviceAsync(deviceId, cancellationToken))
                .ThrowsAsync(new KeyNotFoundException("some exception"));

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FetchSingleDevice(
                deviceServiceMock.Object,
                deviceId,
                cancellationToken
            );

            // Assert
            Assert.NotNull(actionResponse);
            Assert.True(actionResponse.Result is NotFoundResult);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchSingleDeviceAsync(deviceId, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task FetchSingleDevice_When_Exception_Exception_Call_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var exception = new Exception("some exception");
            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchSingleDeviceAsync(deviceId, cancellationToken))
                .ThrowsAsync(exception);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FetchSingleDevice(
                deviceServiceMock.Object,
                deviceId,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchSingleDeviceAsync(deviceId, cancellationToken),
                times: Times.Once
            );
        }

        #endregion

        #region FetchAllDevices

        [Fact]
        public async Task FetchAllDevices_Must_Ok()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var deviceList = new List<DeviceEntity>
            {
                new DeviceEntity
                {
                    Brand = "Apple",
                    Name = "IPHONE 17 PRO MAX",
                    State = DeviceStateEnum.Available
                }
            };

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchAllDevicesAsync(cancellationToken))
                .ReturnsAsync(deviceList);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevices(
                deviceServiceMock.Object,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(deviceList, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchAllDevicesAsync(cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task FetchAllDevices_When_Exception_Exception_Call_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var deviceList = new List<DeviceEntity>
            {
                new DeviceEntity
                {
                    Brand = "Apple",
                    Name = "IPHONE 17 PRO MAX",
                    State = DeviceStateEnum.Available
                }
            };

            var exception = new Exception("some exception");

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchAllDevicesAsync(cancellationToken))
                .ThrowsAsync(exception);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevices(
                deviceServiceMock.Object,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchAllDevicesAsync(cancellationToken),
                times: Times.Once
            );
        }

        #endregion

        #region FetchAllDevicesByBrand

        [Fact]
        public async Task FetchAllDevicesByBrand_Must_Ok()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var brand = "Apple";

            var deviceList = new List<DeviceEntity>
            {
                new DeviceEntity
                {
                    Brand = brand,
                    Name = "IPHONE 17 PRO MAX",
                    State = DeviceStateEnum.Available
                }
            };

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchAllDevicesByBrandAsync(brand, cancellationToken))
                .ReturnsAsync(deviceList);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevicesByBrandAsync(
                deviceServiceMock.Object,
                brand,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(deviceList, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchAllDevicesByBrandAsync(brand, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task FetchAllDevicesByBrandAsync_When_Exception_Exception_Call_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var brand = "Apple";

            var deviceList = new List<DeviceEntity>
            {
                new DeviceEntity
                {
                    Brand = brand,
                    Name = "IPHONE 17 PRO MAX",
                    State = DeviceStateEnum.Available
                }
            };

            var exception = new Exception("some exception");

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchAllDevicesByBrandAsync(brand, cancellationToken))
                .ThrowsAsync(exception);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevicesByBrandAsync(
                deviceServiceMock.Object,
                brand,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchAllDevicesByBrandAsync(brand, cancellationToken),
                times: Times.Once
            );
        }

        #endregion

        #region FetchAllDevicesByState

        [Fact]
        public async Task FetchAllDevicesByStateAsync_Must_Ok()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var state = DeviceStateEnum.Available;

            var deviceList = new List<DeviceEntity>
            {
                new DeviceEntity
                {
                    Brand = "Apple",
                    Name = "IPHONE 17 PRO MAX",
                    State = state
                }
            };

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchAllDevicesByStateAsync(state, cancellationToken))
                .ReturnsAsync(deviceList);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevicesByStateAsync(
                deviceServiceMock.Object,
                state,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(deviceList, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchAllDevicesByStateAsync(state, cancellationToken),
                times: Times.Once
            );
        }

        [Fact]
        public async Task FetchAllDevicesByStateAsync_When_Exception_Exception_Call_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var state = DeviceStateEnum.Available;

            var deviceList = new List<DeviceEntity>
            {
                new DeviceEntity
                {
                    Brand = "Apple",
                    Name = "IPHONE 17 PRO MAX",
                    State = state
                }
            };

            var exception = new Exception("some exception");

            var loggerMock = new Mock<ILogger<DevicesController>>();

            var deviceServiceMock = new Mock<IDevicesCrudService>();
            deviceServiceMock
                .Setup(x => x.FetchAllDevicesByStateAsync(state, cancellationToken))
                .ThrowsAsync(exception);

            var controller = new DevicesController(
                loggerMock.Object
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevicesByStateAsync(
                deviceServiceMock.Object,
                state,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            deviceServiceMock.Verify(
                expression: repo => repo.FetchAllDevicesByStateAsync(state, cancellationToken),
                times: Times.Once
            );
        }

        #endregion
    }
}
