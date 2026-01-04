using System.Runtime.Serialization;

namespace kitchenview.Models
{
    [DataContract]
    public class AppointmentConfiguration
    {
        [DataMember(Name = "Label", IsRequired = true)]
        public required string Label
        {
            get; set;
        }

        [DataMember(Name = "ColorCode", IsRequired = true)]
        public required string ColorCode
        {
            get; set;
        }

        [DataMember(Name = "Url", IsRequired = true)]
        public required string Url
        {
            get; set;
        }
    }
}