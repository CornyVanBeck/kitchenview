using System.Collections.Generic;

namespace kitchenview.Models
{
    public class Week
    {
        public int WeekInMonth
        {
            get; set;
        }

        public int CalendarWeek
        {
            get; set;
        }

        public bool IsCurrent
        {
            get; set;
        }

        public IEnumerable<Day>? Days
        {
            get; set;
        }
    }
}