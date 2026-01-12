using Azure.Core;
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
            catch(KeyNotFoundException ex)
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
                    return NotFound();
                }

                DeviceResponseDto response = new DeviceResponseDto(device);

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error to fetch device by Id. " +
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
        public async Task<ActionResult<IEnumerable<DeviceResponseDto>>> FetchAllDevices(
            [FromServices] IDevicesCrudService service,
            CancellationToken cancellationToken
            )
        {
            try
            {
                IEnumerable<DeviceEntity> devices = await service.FetchAllDevicesAsync(
                    cancellationToken
                );

                List<DeviceResponseDto> response = devices?.Select(d => new DeviceResponseDto(d))?.ToList() ?? [];

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

        [HttpGet("byBrand")]
        public async Task<ActionResult<IEnumerable<DeviceResponseDto>>> FetchAllDevicesByBrandAsync(
            [FromServices] IDevicesCrudService service,
            [FromQuery] string? brand,
            CancellationToken cancellationToken
            )
        {
            try
            {
                IEnumerable<DeviceEntity> devices = await service.FetchAllDevicesByBrandAsync(
                    deviceBrand: brand ?? string.Empty,
                    cancellationToken: cancellationToken
                );

                List<DeviceResponseDto> response = devices?.Select(d => new DeviceResponseDto(d))?.ToList() ?? [];

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to fetch all devices by brandh. " +
                    "Error: {error}",
                    ex.Message
                );

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("byState")]
        public async Task<ActionResult<IEnumerable<DeviceResponseDto>>> FetchAllDevicesByStateAsync(
            [FromServices] IDevicesCrudService service,
            [FromQuery] DeviceStateEnum? state,
            CancellationToken cancellationToken
            )
        {
            try
            {
                IEnumerable<DeviceEntity> devices = await service.FetchAllDevicesByStateAsync(
                    deviceState: state ?? default,
                    cancellationToken: cancellationToken
                );

                List<DeviceResponseDto> response = devices?.Select(d => new DeviceResponseDto(d))?.ToList() ?? [];

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to fetch all devices by state. " +
                    "Error: {error}",
                    ex.Message
                );

                return BadRequest(ex.Message);
            }
        }
    }
}
