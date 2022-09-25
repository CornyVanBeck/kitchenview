using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace kitchenview.Controls.Views
{
    public partial class AwesomeWeatherView : UserControl
    {
        public AwesomeWeatherView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}