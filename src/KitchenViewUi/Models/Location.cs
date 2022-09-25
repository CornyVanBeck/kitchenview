namespace kitchenview.Models
{
    public class Location
    {
        public double Longitude
        {
            get; set;
        }

        public double Latitude
        {
            get; set;
        }

        public string? StreetName
        {
            get; set;
        }

        public string? StreetNumber
        {
            get; set;
        }

        public string? ZipCode
        {
            get; set;
        }

        public string? City
        {
            get; set;
        }

        public string? Country
        {
            get; set;
        }
    }
}