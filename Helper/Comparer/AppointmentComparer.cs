using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using kitchenview.Models;

namespace kitchenview.Helper.Comparer
{
    public class AppointmentComparer : IEqualityComparer<Appointment>
    {
        public bool Equals(Appointment? x, Appointment? y)
        {
            if (x is null || y is null)
            {
                return false;
            }

            if (x.IsRepeatingEventOnly)
            {
                return true;
            }

            bool result = x.Title == y.Title &&
                    x.DateTo == y.DateTo &&
                    x.DateFrom == y.DateFrom &&
                    x.TimeTo == y.TimeTo &&
                    x.TimeFrom == y.TimeFrom;
            return result;
        }

        public bool DayInsideAppointmentWindow(Appointment appointment, int day)
        {
            if (appointment.DateFrom?.Day == appointment.DateTo?.Day)
            {
                return appointment.DateFrom?.Day == day;
            }

            return (day >= appointment.DateFrom?.Day && day <= appointment.DateTo?.Day);
        }

        public int GetHashCode([DisallowNull] Appointment obj)
        {
            return 1;
        }
    }
}