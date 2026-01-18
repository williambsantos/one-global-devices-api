using System.Runtime.Serialization;

namespace OneGlobalDevicesApi.Domain.Entities
{
    public enum DeviceStateEnum
    {
        /// <summary>
        /// Available
        /// </summary>
        [EnumMember(Value = "Available")]
        Available,

        /// <summary>
        /// In Use
        /// </summary>
        [EnumMember(Value = "In Use")]
        InUse,

        /// <summary>
        /// Inactive
        /// </summary>
        [EnumMember(Value = "Inactive")]
        Inactive
    }
}
