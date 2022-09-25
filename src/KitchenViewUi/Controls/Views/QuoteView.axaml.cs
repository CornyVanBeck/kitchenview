using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace kitchenview.Controls.Views
{
    public partial class QuoteView : UserControl
    {
        public QuoteView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}