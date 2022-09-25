using kitchenview.DataAccess;
using kitchenview.Models;
using kitchenview.ViewModels;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using System.Collections.ObjectModel;

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

        public AwesomeGalleryViewModel(IConfiguration configuration, IDataAccess<PhotoprismImage> dataAccess)
        {
            _tiles = new ObservableCollection<AwesomeGalleryTileViewModel>();
            var response = dataAccess.GetData();
            response?.Wait();
            foreach (IGalleryImage image in response?.Result)
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