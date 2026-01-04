using System;
using Avalonia.Threading;
using kitchenview.ViewModels;
using Microsoft.Extensions.Configuration;
using ReactiveUI;

namespace kitchenview.Controls.ViewModels
{
    public class AwesomeCountdownViewModel : ViewModelBase
    {
        private readonly IConfiguration _configuration;

        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private string _awesomeCountdown;

        public string AwesomeCountdown
        {
            get => _awesomeCountdown;
            private set => this.RaiseAndSetIfChanged(ref _awesomeCountdown, value);
        }

        private TimeSpan _countdown;

        public AwesomeCountdownViewModel(IConfiguration configuration)
        {
            _configuration = configuration;

            _countdown = TimeSpan.Parse("01.03:45:52.789");

            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            _countdown = _countdown.Subtract(TimeSpan.FromSeconds(1));
            AwesomeCountdown = _countdown.ToString(@"dd\.hh\:mm\:ss");
        }
    }
}