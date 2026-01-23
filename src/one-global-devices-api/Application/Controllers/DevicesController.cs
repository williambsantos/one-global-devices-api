using Microsoft.AspNetCore.Mvc;
using OneGlobalDevicesApi.Application.DTOs;
using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Services;

namespace OneGlobalDevicesApi.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(ILogger<DevicesController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<DeviceResponseDto>> CreateNewDeviceAsync(
            [FromServices] IDevicesCrudService service,
            [FromBody] DeviceCreateRequestDTO request,
            CancellationToken cancellationToken)
        {
            try
            {
                DeviceEntity deviceCreated = await service.CreateNewDeviceAsync(
                    name: request.Name,
                    brand: request.Brand,
                    cancellationToken: cancellationToken
                );

                DeviceResponseDto response = new DeviceResponseDto(deviceCreated);

                return CreatedAtAction(nameof(FetchSingleDevice), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to create new device. " +
                    "name: {name}. " +
                    "brand: {brand}. " +
                    "error: {error}",
                    request?.Name,
                    request?.Brand,
                    ex.Message
                );

                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DeviceResponseDto>> FullyUpdateDeviceAsync(
            [FromServices] IDevicesCrudService service,
            Guid id,
            [FromBody] DeviceFullyUpdateRequestDTO request,
            CancellationToken cancellationToken)
        {
            try
            {
                DeviceEntity deviceCreated = await service.UpdateDeviceAsync(
                    id: id,
                    newName: request.NewName,
                    newBrand: request.NewBrand,
                    newState: request.NewState,
                    cancellationToken: cancellationToken
                );

                DeviceResponseDto response = new DeviceResponseDto(deviceCreated);

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error to fully update a device. NotFound. " +
                    "id: {id}. " +
                    "name: {name}. " +
                    "brand: {brand}. " +
                    "state: {state}. " +
                    "error: {error}",
                    id,
                    request?.NewName,
                    request?.NewBrand,
                    request?.NewState,
                    ex.Message
                );

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to fully update a device. " +
                    "id: {id}. " +
                    "name: {name}. " +
                    "brand: {brand}. " +
                    "state: {state}. " +
                    "error: {error}",
                    id,
                    request?.NewName,
                    request?.NewBrand,
                    request?.NewState,
                    ex.Message
                );

                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<DeviceResponseDto>> PartiallyUpdateDeviceAsync(
            [FromServices] IDevicesCrudService service,
            Guid id,
            [FromBody] DevicePartiallyUpdateRequestDTO request,
            CancellationToken cancellationToken)
        {
            try
            {
                DeviceEntity deviceCreated = await service.UpdateDeviceAsync(
                    id: id,
                    newName: request.NewName,
                    newBrand: request.NewBrand,
                    newState: request.NewState,
                    cancellationToken: cancellationToken
                );

                DeviceResponseDto response = new DeviceResponseDto(deviceCreated);

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error to partially update a device. NotFound. " +
                    "id: {id}. " +
                    "name: {name}. " +
                    "brand: {brand}. " +
                    "state: {state}. " +
                    "error: {error}",
                    id,
                    request?.NewName,
                    request?.NewBrand,
                    request?.NewState,
                    ex.Message
                );

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to partially update a device. " +
                    "id: {id}. " +
                    "name: {name}. " +
                    "brand: {brand}. " +
                    "state: {state}. " +
                    "error: {error}",
                    id,
                    request?.NewName,
                    request?.NewBrand,
                    request?.NewState,
                    ex.Message
                );

                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSingleDevice(
            [FromServices] IDevicesCrudService service,
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                await service.DeleteSingleDeviceAsync(
                    id: id,
                    cancellationToken: cancellationToken
                );

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error to delete a single device. " +
                    "id: {id}. " +
                    "error: {error}",
                    id,
                    ex.Message
                );

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to delete a single device. " +
                    "id: {id}. " +
                    "error: {error}",
                    id,
                    ex.Message
                );

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceResponseDto>> FetchSingleDevice(
            [FromServices] IDevicesCrudService service,
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                DeviceEntity? device = await service.FetchSingleDeviceAsync(
                    id: id,
                    cancellationToken: cancellationToken
                );
                if (device == null)
                {
                    _logger.LogWarning("Device not found. " +
                        "id: {id}. " +
                        id
                    );

                    return NotFound();
                }

                DeviceResponseDto response = new DeviceResponseDto(device);

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Device not found. " +
                    "id: {id}. " +
                    "error: {error}",
                    id,
                    ex.Message
                );

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to fetch device by Id. " +
                    "id: {id}. " +
                    "Error: {error}",
                    id,
                    ex.Message
                );

                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<DeviceResponseDto>>> FetchAllDevices(
            [FromServices] IDevicesCrudService service,
            [FromQuery] string? brand,
            [FromQuery] DeviceStateEnum? state,
            [FromQuery] PaginationRequestDTO? pagination,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                var paginationRequest = pagination?.ToDomainEntity() ?? new PaginationRequest();

                PaginationResponse<DeviceEntity> devices = await service.FetchAllDevicesAsync(
                    brand, state,
                    paginationRequest,
                    cancellationToken: cancellationToken
                );

                PaginationResponse<DeviceResponseDto> response = devices.ConvertContentTo(
                    fnConvertMethod: deviceEntity => new DeviceResponseDto(deviceEntity)
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to fetch all devices. " +
                    "Error: {error}",
                    ex.Message
                );

                return BadRequest(ex.Message);
            }
        }
    }
}
