using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace kitchenview.Controls.Views
{
    public partial class AwesomeGalleryView : UserControl
    {
        public AwesomeGalleryView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}