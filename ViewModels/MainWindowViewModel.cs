using System;
using kitchenview.Models;
using kitchenview.Controls.ViewModels;
using Splat;
using kitchenview.DataAccess;
using Microsoft.Extensions.Configuration;
using Avalonia.Threading;
using System.ComponentModel;
using ReactiveUI;

namespace kitchenview.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IConfiguration configuration;

        private readonly DispatcherTimer _calendarTimer = new DispatcherTimer();

        private readonly DispatcherTimer _wordClockTimer = new DispatcherTimer();
        private readonly DispatcherTimer _weatherClockTimer = new DispatcherTimer();


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

        public AwesomeCountdownViewModel AwesomeCountdown { get; }

        public MainWindowViewModel(IConfiguration configuration)
        {
            AwesomeIndex = 0;
            _calendarTimer.Tick += OnCalendarTick;
#if DEBUG
            _calendarTimer.Interval = TimeSpan.FromSeconds(2);
#else
            _calendarTimer.Interval = configuration.GetValue<TimeSpan>("Controls:Calendars:ViewTimer");  
#endif
            _calendarTimer.Start();

            //_wordClockTimer.Tick += OnWordClockTick;
            //_wordClockTimer.Interval = configuration.GetValue<TimeSpan>("Controls:WordClock:ViewTimer");
            _weatherClockTimer.Tick += OnWeatherTick;
#if DEBUG
            _weatherClockTimer.Interval = TimeSpan.FromSeconds(30);
#else
            _weatherClockTimer.Interval = configuration.GetValue<TimeSpan>("Controls:Weather:ViewTimer");  
#endif

            this.configuration = configuration;
            var calendarService = Locator.Current.GetService<IDataAccess<Appointment>>();
            var weatherService = Locator.Current.GetService<IDataAccess<Weather>>();
            //var quoteService = Locator.Current.GetService<IDataAccess<IQuote>>();

            AwesomeCalendar = new AwesomeCalendarViewModel(configuration, calendarService!);
            AwesomeWeather = new AwesomeWeatherViewModel(configuration, weatherService!);
            //AwesomeWordClock = new AwesomeWordClockViewModel(configuration);
            //Quote = new QuoteViewModel(configuration, quoteService!);
        }

        private void OnCalendarTick(object? sender, EventArgs e)
        {
            AwesomeIndex = 1;
            _calendarTimer.Stop();
            _weatherClockTimer.Start();
        }

        private void OnWordClockTick(object? sender, EventArgs e)
        {
            AwesomeIndex = 0;
            _wordClockTimer.Stop();
            _calendarTimer.Start();
        }

        private void OnWeatherTick(object? sender, EventArgs e)
        {
            AwesomeIndex = 0;
            _weatherClockTimer.Stop();
            _calendarTimer.Start();
        }
    }
}
