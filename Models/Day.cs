using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace kitchenview.Models
{
    public class Day
    {
        public int DayOfMonth
        {
            get; set;
        }

        public int DayOfWeek
        {
            get;
            set;
        }

        public string? Name
        {
            get;
            set;
        }

        public bool IsCurrent
        {
            get; set;
        }

        public bool HasBirthday
        {
            get; set;
        }

        public ObservableCollection<Appointment>? Appointments
        {
            get; set;
        }

        public bool IsEnabled
        {
            get; set;
        }
    }
}