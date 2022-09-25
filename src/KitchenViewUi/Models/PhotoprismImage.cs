using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace kitchenview.Models
{
    public class PhotoprismImage : IGalleryImage
    {
        private RestClient client;

        private string CachePath;

        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                CachePath = $"./Cache/{value}.jpeg";
            }
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

        public PhotoprismImage(RestClient client)
        {
            this.client = client;
        }

        public async Task<Stream> LoadImageBitmapAsync(string url)
        {
            if (File.Exists(CachePath))
            {
                return File.OpenRead(CachePath);
            }
            else
            {
                var request = new RestRequest(url);
                var data = await client?.GetAsync(request);

                SaveAsync(data!.RawBytes);
                return new MemoryStream(data!.RawBytes);
            }
        }

        internal async Task SaveAsync(byte[] data)
        {
            if (!Directory.Exists("./Cache"))
            {
                Directory.CreateDirectory("./Cache");
            }

            using (var fs = File.OpenWrite(CachePath))
            {
                await fs.WriteAsync(data, 0, data.Length);
            }
        }

        internal static async Task SaveToStreamAsync(PhotoprismImage data, Stream stream)
        {
            await JsonSerializer.SerializeAsync(stream, data).ConfigureAwait(false);
        }
    }
}