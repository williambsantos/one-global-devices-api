using System.Runtime.Serialization;

namespace OneGlobalDevicesApi.Domain.Exceptions
{
    public class DeviceBusinessException : Exception
    {
        public DeviceBusinessException()
        {
        }

        public DeviceBusinessException(string? message) : base(message)
        {
        }

        public DeviceBusinessException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
