using System.Collections.Generic;

namespace kitchenview.Models
{
    public interface IGallery
    {
        public string Title
        {
            get; set;
        }

        public IEnumerable<IGalleryImage> Images
        {
            get; set;
        }
    }
}