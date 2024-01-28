using System;
using kitchenview.Models;
using kitchenview.Controls.ViewModels;
using Splat;
using kitchenview.DataAccess;
using Microsoft.Extensions.Configuration;
using Avalonia.Threading;
using System.Diagnostics;
using System.ComponentModel;
using ReactiveUI;

namespace kitchenview.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IConfiguration configuration;

        private readonly DispatcherTimer _calendarTimer = new DispatcherTimer();
        private readonly DispatcherTimer _wordClockTimer = new DispatcherTimer();

        private int _awesomeIndex;

        public int AwesomeIndex
        {
            get => _awesomeIndex;
            private set => this.RaiseAndSetIfChanged(ref _awesomeIndex, value);
        }

        public AwesomeCalendarViewModel AwesomeCalendar { get; }

        public QuoteViewModel Quote { get; }

        public AwesomeGalleryViewModel AwesomeGallery { get; }

        public AwesomeShoppingListViewModel AwesomeShoppingList { get; }

        public AwesomeWeatherViewModel AwesomeWeather { get; }

        public AwesomeWordClockViewModel AwesomeWordClock { get; }

        public MainWindowViewModel(IConfiguration configuration)
        {
            AwesomeIndex = 0;
            _calendarTimer.Tick += OnCalendarTick;
            _calendarTimer.Interval = configuration.GetValue<TimeSpan>("Controls:Calendars:ViewTimer");
            _calendarTimer.Start();

            _wordClockTimer.Tick += OnWordClockTick;
            _wordClockTimer.Interval = configuration.GetValue<TimeSpan>("Controls:WordClock:ViewTimer");

            this.configuration = configuration;
            var calendarService = Locator.Current.GetService<IDataAccess<Appointment>>();
            //var quoteService = Locator.Current.GetService<IDataAccess<IQuote>>();
            //var galleryService = Locator.Current.GetService<IDataAccess<PhotoprismImage>>();
            //var shoppingListService = Locator.Current.GetService<IDataAccess<TodoistShoppingListEntry>>();

            AwesomeCalendar = new AwesomeCalendarViewModel(configuration, calendarService!);
            AwesomeWordClock = new AwesomeWordClockViewModel(configuration);
            //Quote = new QuoteViewModel(configuration, quoteService!);
            //AwesomeGallery = new AwesomeGalleryViewModel(configuration, galleryService!);
            //AwesomeShoppingList = new AwesomeShoppingListViewModel(configuration, shoppingListService!);
            //AwesomeWeather = new AwesomeWeatherViewModel();
        }

        private void OnCalendarTick(object? sender, EventArgs e)
        {
            AwesomeIndex = 1;
            _calendarTimer.Stop();
            _wordClockTimer.Start();
        }

        private void OnWordClockTick(object? sender, EventArgs e)
        {
            AwesomeIndex = 0;
            _wordClockTimer.Stop();
            _calendarTimer.Start();
        }
    }
}
