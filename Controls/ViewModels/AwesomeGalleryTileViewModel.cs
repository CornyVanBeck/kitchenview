using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using kitchenview.Models;
using kitchenview.ViewModels;
using ReactiveUI;

namespace kitchenview.Controls.ViewModels
{
    public class AwesomeGalleryTileViewModel : ViewModelBase
    {
        private IGalleryImage? image;

        public AwesomeGalleryTileViewModel(IGalleryImage image)
        {
            this.image = image;
        }

        private Bitmap? _loadedImage;

        public Bitmap? LoadedImage
        {
            get => _loadedImage;
            private set => this.RaiseAndSetIfChanged(ref _loadedImage, value);
        }

        public string Url
        {
            get; set;
        }

        public async Task LoadImage()
        {
            await using (var imageStream = await image.LoadImageBitmapAsync(image?.Url))
            {
                LoadedImage = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
            }
        }
    }
}