using System;

namespace kitchenview.Models
{
    public class Appointment
    {
        public string? Title
        {
            get; set;
        }

        public DateTime? DateFrom
        {
            get; set;
        }

        public DateTime? DateTo
        {
            get; set;
        }

        public TimeSpan? TimeFrom
        {
            get; set;
        }

        public TimeSpan? TimeTo
        {
            get; set;
        }

        public Location? Location
        {
            get; set;
        }

        public string ColorCode
        {
            get; set;
        }

        public bool IsLongTitle
        {
            get; set;
        }


    }
}