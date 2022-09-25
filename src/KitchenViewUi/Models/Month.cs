using System.Collections.Generic;
using Avalonia.Controls;

namespace kitchenview.Models
{
    public class Month
    {
        public int Value
        {
            get; set;
        }

        public string? Name
        {
            get; set;
        }

        public Image? Header
        {
            get; set;
        }

        public IEnumerable<Week>? Weeks
        {
            get; set;
        }
    }
}