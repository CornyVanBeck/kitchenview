using System.Runtime.Serialization;

namespace kitchenview.Models
{
    [DataContract]
    public class AppointmentConfiguration
    {
        [DataMember(Name = "ColorCode", IsRequired = true)]
        public string ColorCode
        {
            get; set;
        }

        [DataMember(Name = "Url", IsRequired = true)]
        public string Url
        {
            get; set;
        }
    }
}