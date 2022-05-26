using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace kitchenview.Controls.Views
{
    public partial class AwesomeGalleryTileView : UserControl
    {
        public AwesomeGalleryTileView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}