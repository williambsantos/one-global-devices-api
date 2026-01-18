namespace OneGlobalDevicesApi.Domain.Exceptions
{
    public class DeviceBusinessException : Exception
    {
        public DeviceBusinessException(string? message) : base(message)
        {
        }
    }
}
