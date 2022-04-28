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

        public QuoteViewModel Quote { get; }

        public MainWindowViewModel(IConfiguration configuration)
        {
            this.configuration = configuration;
            var calendarService = Locator.Current.GetService<IDataAccess<Appointment>>();
            var quoteService = Locator.Current.GetService<IDataAccess<IQuote>>();
            AwesomeCalendar = new AwesomeCalendarViewModel(configuration, calendarService!);
            Quote = new QuoteViewModel(configuration, quoteService!);
        }
    }
}
