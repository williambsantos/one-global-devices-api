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

        /*
        Create a new device. 
        Fully and/or partially update an existing device. 
        Fetch a single device. 
        Fetch all devices. 
        Fetch devices by brand. 
        Fetch devices by state. 
        Delete a single device. 
        */

        [HttpPost]
        public async Task<ActionResult<DeviceResponseDto>> CreateNewDeviceAsync(
            [FromServices] IDevicesCrudService service,
            [FromBody] DeviceCreateRequestDTO request)
        {
            try
            {
                DeviceEntity deviceCreated = await service.CreateNewDeviceAsync(request.Name, request.Brand);

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
            [FromBody] DeviceFullyUpdateRequestDTO request)
        {
            try
            {
                DeviceEntity deviceCreated = await service.UpdateDeviceAsync(
                    id: id, 
                    newName: request.NewName,
                    newBrand: request.NewBrand,
                    newState: request.NewState
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
            [FromBody] DevicePartiallyUpdateRequestDTO request)
        {
            try
            {
                DeviceEntity deviceCreated = await service.UpdateDeviceAsync(
                    id: id,
                    newName: request.NewName,
                    newBrand: request.NewBrand,
                    newState: request.NewState
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
        public async Task<ActionResult<DeviceResponseDto>> DeleteSingleDevice(
            [FromServices] IDevicesCrudService service,
            Guid id)
        {
            try
            {
                await service.DeleteSingleDeviceAsync(id);

                return NoContent();
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
            Guid id)
        {
            try
            {
                DeviceEntity device = await service.FetchSingleDeviceAsync(id);
                if (device == null)
                {
                    return NotFound(new { message = $"Device with ID {id} not found" });
                }

                DeviceResponseDto response = new DeviceResponseDto(device);

                return Ok(response);
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
            [FromServices] IDevicesCrudService service
            )
        {
            try
            {
                IEnumerable<DeviceEntity> devices = await service.FetchAllDevicesAsync();

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
        public async Task<ActionResult<IEnumerable<DeviceResponseDto>>> FetchAllDevicesByBrand(
            [FromServices] IDevicesCrudService service,
            [FromQuery] string? brand
            )
        {
            try
            {
                IEnumerable<DeviceEntity> devices = await service.FetchAllByBrandAsync(brand ?? string.Empty);

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
        public async Task<ActionResult<IEnumerable<DeviceResponseDto>>> FetchAllDevicesByState(
            [FromServices] IDevicesCrudService service,
            [FromQuery] DeviceStateEnum? state
            )
        {
            try
            {
                IEnumerable<DeviceEntity> devices = await service.FetchAllByStateAsync(state ?? default);

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
