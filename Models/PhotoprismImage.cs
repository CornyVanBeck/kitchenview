using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace kitchenview.Models
{
    public class PhotoprismImage : IGalleryImage
    {
        private static HttpClient client = new();

        private string CachePath => "./Cache/{Artist} - {Title}";
        
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


        public async Task<Stream> LoadImageBitmapAsync(string url)
        {
            if (File.Exists(CachePath + ".jpeg"))
            {
                return File.OpenRead(CachePath + ".jpeg");
            }
            else
            {
                var data = await client.GetByteArrayAsync(url);

                return new MemoryStream(data);
            }
        }

        /* internal async Task SaveAsync()
        {
            if (!Directory.Exists("./Cache"))
            {
                Directory.CreateDirectory("./Cache");
            }

            using (var fileStreamToSave = File.OpenWrite(CachePath))
            {
                await SaveToStreamAsync(fileStreamToSave);
            }
        }

        internal static async Task SaveToStreamAsync(Stream stream)
        {
            await JsonConvert.SerializeObject(stream);
        } */
    }
}