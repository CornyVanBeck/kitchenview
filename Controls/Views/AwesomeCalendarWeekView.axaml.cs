using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace kitchenview.Controls.Views
{
    public partial class AwesomeCalendarWeekView : UserControl
    {
        public AwesomeCalendarWeekView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}