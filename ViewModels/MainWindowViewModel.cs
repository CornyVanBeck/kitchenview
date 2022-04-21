using System;
using System.Collections.Generic;
using System.Text;
using kitchenview.Models;
using System.Linq;
using System.Globalization;
using kitchenview.Controls.ViewModels;
using Splat;
using kitchenview.DataAccess;
using Microsoft.Extensions.Configuration;

namespace kitchenview.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IConfiguration configuration;

        public AwesomeCalendarViewModel AwesomeCalendar { get; }

        public MainWindowViewModel(IConfiguration configuration)
        {
            this.configuration = configuration;
            var services = Locator.Current.GetServices<IDataAccess<Appointment>>();
            AwesomeCalendar = new AwesomeCalendarViewModel(configuration, services.Where(service => service is IcsCalendarDataAccess).FirstOrDefault()!);
        }
    }
}
