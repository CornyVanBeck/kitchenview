using System.Collections.Generic;
using System.Collections.ObjectModel;
using kitchenview.Models;
using kitchenview.ViewModels;
using ReactiveUI;

namespace kitchenview.Controls.ViewModels
{
    public class AwesomeGalleryViewModel : ViewModelBase
    {
        private ObservableCollection<AwesomeGalleryTileViewModel> _tiles;

        public ObservableCollection<AwesomeGalleryTileViewModel> Tiles
        {
            get => _tiles;
            set => this.RaiseAndSetIfChanged(ref _tiles, value);
        }

        public ObservableCollection<IGalleryImage> GallerySource { get; set; }

        public AwesomeGalleryViewModel()
        {
            GallerySource = new ObservableCollection<IGalleryImage>()
            {
                new PhotoprismImage()
                {
                    Url = "https://www.iliketowastemytime.com/sites/default/files/forest-wallpaper-2880x1800.jpg"
                },
                new PhotoprismImage()
                {
                    Url = "https://www.pixelstalk.net/wp-content/uploads/2016/10/bamboo_forest-wallpaper-1920x1200-620x378.jpg"
                },
                new PhotoprismImage()
                {
                    Url = "https://www.pixelstalk.net/wp-content/uploads/2016/10/Free-pine-forest-wallpaper-620x388.jpg"
                },
            };

            _tiles = new ObservableCollection<AwesomeGalleryTileViewModel>();
            foreach (IGalleryImage image in GallerySource)
            {
                if (image is PhotoprismImage photo)
                {
                    _tiles.Add(new AwesomeGalleryTileViewModel(photo));
                }
            }

            foreach (AwesomeGalleryTileViewModel tile in _tiles)
            {
                tile.LoadImage();
            }
        }
    }
}