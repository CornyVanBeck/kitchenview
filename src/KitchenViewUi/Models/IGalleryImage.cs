using System;
using System.IO;
using System.Threading.Tasks;

namespace kitchenview.Models
{
    public interface IGalleryImage
    {
        public string Name
        {
            get; set;
        }

        public string FileType
        {
            get; set;
        }

        public string Url
        {
            get; set;
        }

        public DateTime Date
        {
            get; set;
        }

        public string Comment
        {
            get; set;
        }

        public Task<Stream> LoadImageBitmapAsync(string url);
    }
}