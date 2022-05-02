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

            bool result = x.Title == y.Title &&
                    x.DateTo == y.DateTo &&
                    x.DateFrom == y.DateFrom &&
                    x.TimeTo == y.TimeTo &&
                    x.TimeFrom == y.TimeFrom;
            return result;
        }

        public int GetHashCode([DisallowNull] Appointment obj)
        {
            return 1;
        }
    }
}