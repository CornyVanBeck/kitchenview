using System;
using System.Timers;
using kitchenview.DataAccess;
using kitchenview.Models;
using kitchenview.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Generic;
using ReactiveUI;

namespace kitchenview.Controls.ViewModels
{
    public class QuoteViewModel : ViewModelBase
    {
        private readonly IConfiguration configuration;

        private readonly IDataAccess<IQuote> quoteDataAccess;

        private readonly Timer timer;

        private string _quote;
        public string Quote
        {
            get => _quote;
            set => this.RaiseAndSetIfChanged(ref _quote, value);
        }

        private string _author;
        public string Author
        {
            get => _author;
            set => this.RaiseAndSetIfChanged(ref _author, value);
        }

        public QuoteViewModel(IConfiguration configuration, IDataAccess<IQuote> quoteDataAccess)
        {
            this.configuration = configuration;
            this.quoteDataAccess = quoteDataAccess;

            timer = new Timer();
            timer.Interval = TimeSpan.Parse(configuration?["Controls:Quotes:Intervall"] ?? "24:00:00").TotalMilliseconds;
            timer.Elapsed += OnTick;
            timer.Start();
            LoadQuote();
        }

        internal void OnTick(object sender, EventArgs args)
        {
            LoadQuote();
        }

        internal void LoadQuote()
        {
            var response = quoteDataAccess?.GetData();
            if (response is not null && response.IsCanceled)
            {
                _quote = "-";
                _author = "-";
                return;
            }
            var quote = (response.Result as List<IQuote>)?.FirstOrDefault();

            _quote = quote?.Quote ?? "~";
            _author = quote?.Author ?? "~";
        }
    }
}