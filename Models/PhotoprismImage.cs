using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace kitchenview.Models
{
    public class PhotoprismImage : IGalleryImage
    {
        private RestClient client;

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

        public PhotoprismImage(RestClient client)
        {
            this.client = client;
        }

        public async Task<Stream> LoadImageBitmapAsync(string url)
        {
            if (File.Exists(CachePath + ".jpeg"))
            {
                return File.OpenRead(CachePath + ".jpeg");
            }
            else
            {
                var request = new RestRequest(url);
                var data = await client?.GetAsync(request);

                return new MemoryStream(data.RawBytes);
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