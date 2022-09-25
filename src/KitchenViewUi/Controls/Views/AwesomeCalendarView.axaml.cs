using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using kitchenview.Controls.ViewModels;

namespace kitchenview.Controls.Views
{
    public partial class AwesomeCalendarView : UserControl
    {
        public AwesomeCalendarView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}