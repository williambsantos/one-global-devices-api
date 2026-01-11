using System.ComponentModel.DataAnnotations;

namespace OneGlobalDevicesApi.Application.DTOs
{
    public class DeviceCreateRequestDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters")]
        public required string Brand { get; set; }
    }
}
