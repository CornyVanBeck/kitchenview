using System.Globalization;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using kitchenview.DataAccess;
using kitchenview.Models;
using kitchenview.ViewModels;
using kitchenview.Views;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Serilog;
using Splat;
using Splat.Serilog;

namespace kitchenview
{
    public partial class App : Application
    {
        private readonly RestClient client = new RestClient();

        private readonly IConfiguration configuration;

        public App()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log-.txt",
                                rollingInterval: RollingInterval.Day,
                                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var builder = new ConfigurationBuilder()
                            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Locator.CurrentMutable.UseSerilogFullLogger();
            Locator.CurrentMutable.RegisterConstant<IDataAccess<Appointment>>(new IcsCalendarDataAccess(configuration, client));
            Locator.CurrentMutable.RegisterConstant<IDataAccess<IQuote>>(new QuoteDataAccess(configuration, client));
            Locator.CurrentMutable.RegisterConstant<IDataAccess<PhotoprismImage>>(new PhotoprismGalleryDataAccess(configuration, client));

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(configuration),
                };
#if DEBUG
#else
                desktop.MainWindow.WindowState = Avalonia.Controls.WindowState.FullScreen;
                desktop.MainWindow.SystemDecorations = Avalonia.Controls.SystemDecorations.None;
#endif
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}