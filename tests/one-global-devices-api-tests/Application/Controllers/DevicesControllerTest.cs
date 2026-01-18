using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
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
            actionResponse.Should().NotBeNull();

            var okResult = actionResponse.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Should().NotBeNull();

            var actual = okResult.Value as IEnumerable<DeviceResponseDto> ??
                throw new InvalidOperationException("Expected a DeviceResponseDto");

            for (int i = 0; i < expectedList.Count(); i++)
            {
                AssertDeviceEntity(expectedList.ElementAt(i), actual.ElementAt(i));
            }
        }

        private void AssertActionResultDeviceResponseDto(DeviceEntity expected, ActionResult<DeviceResponseDto> actionResponse)
        {
            actionResponse.Should().NotBeNull();

            var okResult = actionResponse.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Should().NotBeNull();

            var actual = okResult.Value as DeviceResponseDto ??
                throw new InvalidOperationException("Expected a DeviceResponseDto");

            AssertDeviceEntity(expected, actual);
        }

        private void AssertDeviceEntity(DeviceEntity expected, DeviceResponseDto actual)
        {
            actual.Id.Should().Be(expected.Id);
            actual.Name.Should().Be(expected.Name);
            actual.Brand.Should().Be(expected.Brand);
            actual.State.Should().Be(expected.State.ToString());
        }

        private void AssertBadRequestObjectResult<T>(Exception exception, ActionResult<T> actionResponse)
        {
            actionResponse.Should().NotBeNull();
            actionResponse.Result.Should().NotBeNull();

            var badRequest = actionResponse.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be(exception.Message);
        }

        private void AssertNotFound<T>(ActionResult<T> actionResponse)
        {
            actionResponse.Should().NotBeNull();
            actionResponse.Result.Should().BeOfType<NotFoundResult>();
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.CreateNewDeviceAsync(name, brand, cancellationToken).Returns(deviceCreated);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.CreateNewDeviceAsync(
                deviceServiceMock,
                dto,
                cancellationToken
            );

            // Assert
            actionResponse.Should().NotBeNull();
            var createdResult = actionResponse.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.Value.Should().NotBeNull();
            createdResult.ActionName.Should().Be(nameof(controller.FetchSingleDevice));

            var actual = createdResult.Value as DeviceResponseDto ?? throw new InvalidOperationException("value is not a DeviceResponseDto");
            AssertDeviceEntity(deviceCreated, actual);

            await deviceServiceMock.Received(1).CreateNewDeviceAsync(name, brand, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.CreateNewDeviceAsync(name, brand, cancellationToken).Returns<DeviceEntity>(_ => throw exception);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.CreateNewDeviceAsync(
                deviceServiceMock,
                dto,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            await deviceServiceMock.Received(1).CreateNewDeviceAsync(name, brand, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken).Returns(device);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FullyUpdateDeviceAsync(
                deviceServiceMock,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(device, actionResponse);

            await deviceServiceMock.Received(1).UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken);
        }

        [Fact]
        public async Task FullyUpdateDeviceAsync_When_KeyNotFound_Log_And_NotFound()
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

            var exception = new KeyNotFoundException("some exception");

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();

            deviceServiceMock
                .UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken)
                .Returns<DeviceEntity>(_ => throw exception);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FullyUpdateDeviceAsync(
                deviceServiceMock,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertNotFound(actionResponse);

            await deviceServiceMock.Received(1).UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken).Returns<DeviceEntity>(_ => throw exception);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FullyUpdateDeviceAsync(
                deviceServiceMock,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            await deviceServiceMock.Received(1).UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken).Returns(device);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.PartiallyUpdateDeviceAsync(
                deviceServiceMock,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(device, actionResponse);

            await deviceServiceMock.Received(1).UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken);
        }

        [Fact]
        public async Task PartiallyUpdateDeviceAsync_When_KeyNotFound_Log_And_NotFound()
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

            var exception = new KeyNotFoundException("some exception");

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();

            deviceServiceMock
                .UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken)
                .Returns<DeviceEntity>(_ => throw exception);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.PartiallyUpdateDeviceAsync(
                deviceServiceMock,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertNotFound(actionResponse);

            await deviceServiceMock.Received(1).UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken).Returns<DeviceEntity>(_ => throw exception);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.PartiallyUpdateDeviceAsync(
                deviceServiceMock,
                deviceId,
                dto,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            await deviceServiceMock.Received(1).UpdateDeviceAsync(deviceId, newName, newBrand, newState, cancellationToken);
        }

        #endregion

        #region DeleteSingleDevice

        [Fact]
        public async Task DeleteSingleDevice_Must_Ok()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.DeleteSingleDevice(
                deviceServiceMock,
                deviceId,
                cancellationToken
            );

            // Assert
            actionResponse.Should().NotBeNull();
            actionResponse.Result.Should().BeOfType<NoContentResult>();

            await deviceServiceMock.Received(1).DeleteSingleDeviceAsync(deviceId, cancellationToken);
        }

        [Fact]
        public async Task DeleteSingleDevice_When_KeyNotFoundException_Log_And_NotFound()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var exception = new KeyNotFoundException("some exception");

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.DeleteSingleDeviceAsync(deviceId, cancellationToken).Returns(Task.FromException(exception));

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.DeleteSingleDevice(
                deviceServiceMock,
                deviceId,
                cancellationToken
            );

            // Assert
            AssertNotFound(actionResponse);

            await deviceServiceMock.Received(1).DeleteSingleDeviceAsync(deviceId, cancellationToken);
        }

        [Fact]
        public async Task DeleteSingleDevice_When_ERROR_Log_And_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var exception = new Exception("some exception");

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.DeleteSingleDeviceAsync(deviceId, cancellationToken).Returns(Task.FromException(exception));

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.DeleteSingleDevice(
                deviceServiceMock,
                deviceId,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            await deviceServiceMock.Received(1).DeleteSingleDeviceAsync(deviceId, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.FetchSingleDeviceAsync(deviceId, cancellationToken).Returns(device);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FetchSingleDevice(
                deviceServiceMock,
                deviceId,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(device, actionResponse);

            await deviceServiceMock.Received(1).FetchSingleDeviceAsync(deviceId, cancellationToken);
        }

        [Fact]
        public async Task FetchSingleDevice_When_Return_Null_Call_NotFound()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            DeviceEntity? device = null;

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.FetchSingleDeviceAsync(deviceId, cancellationToken).Returns(device);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FetchSingleDevice(
                deviceServiceMock,
                deviceId,
                cancellationToken
            );

            // Assert
            actionResponse.Should().NotBeNull();
            actionResponse.Result.Should().BeOfType<NotFoundResult>();

            await deviceServiceMock.Received(1).FetchSingleDeviceAsync(deviceId, cancellationToken);
        }

        [Fact]
        public async Task FetchSingleDevice_When_Exception_KeyNotFound_Call_NotFound()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock
                .FetchSingleDeviceAsync(deviceId, cancellationToken)
                .Returns(Task.FromException<DeviceEntity?>(new KeyNotFoundException("some exception")));

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FetchSingleDevice(
                deviceServiceMock,
                deviceId,
                cancellationToken
            );

            // Assert
            actionResponse.Should().NotBeNull();
            actionResponse.Result.Should().BeOfType<NotFoundResult>();

            await deviceServiceMock.Received(1).FetchSingleDeviceAsync(deviceId, cancellationToken);
        }

        [Fact]
        public async Task FetchSingleDevice_When_Exception_Exception_Call_BadRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var exception = new Exception("some exception");
            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.FetchSingleDeviceAsync(deviceId, cancellationToken).Returns(Task.FromException<DeviceEntity?>(exception));

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<DeviceResponseDto> actionResponse = await controller.FetchSingleDevice(
                deviceServiceMock,
                deviceId,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            await deviceServiceMock.Received(1).FetchSingleDeviceAsync(deviceId, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.FetchAllDevicesAsync(cancellationToken).Returns(deviceList);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevices(
                deviceServiceMock,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(deviceList, actionResponse);

            await deviceServiceMock.Received(1).FetchAllDevicesAsync(cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.FetchAllDevicesAsync(cancellationToken).Returns(Task.FromException<IEnumerable<DeviceEntity>>(exception));

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevices(
                deviceServiceMock,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            await deviceServiceMock.Received(1).FetchAllDevicesAsync(cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.FetchAllDevicesByBrandAsync(brand, cancellationToken).Returns(deviceList);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevicesByBrandAsync(
                deviceServiceMock,
                brand,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(deviceList, actionResponse);

            await deviceServiceMock.Received(1).FetchAllDevicesByBrandAsync(brand, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.FetchAllDevicesByBrandAsync(brand, cancellationToken).Returns(Task.FromException<IEnumerable<DeviceEntity>>(exception));

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevicesByBrandAsync(
                deviceServiceMock,
                brand,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            await deviceServiceMock.Received(1).FetchAllDevicesByBrandAsync(brand, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.FetchAllDevicesByStateAsync(state, cancellationToken).Returns(deviceList);

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevicesByStateAsync(
                deviceServiceMock,
                state,
                cancellationToken
            );

            // Assert
            AssertActionResultDeviceResponseDto(deviceList, actionResponse);

            await deviceServiceMock.Received(1).FetchAllDevicesByStateAsync(state, cancellationToken);
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

            var loggerMock = Substitute.For<ILogger<DevicesController>>();

            var deviceServiceMock = Substitute.For<IDevicesCrudService>();
            deviceServiceMock.FetchAllDevicesByStateAsync(state, cancellationToken).Returns(Task.FromException<IEnumerable<DeviceEntity>>(exception));

            var controller = new DevicesController(
                loggerMock
            );

            // Act
            ActionResult<IEnumerable<DeviceResponseDto>> actionResponse = await controller.FetchAllDevicesByStateAsync(
                deviceServiceMock,
                state,
                cancellationToken
            );

            // Assert
            AssertBadRequestObjectResult(exception, actionResponse);

            await deviceServiceMock.Received(1).FetchAllDevicesByStateAsync(state, cancellationToken);
        }

        #endregion
    }
}
