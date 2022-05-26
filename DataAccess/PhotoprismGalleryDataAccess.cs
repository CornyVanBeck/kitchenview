using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Proxies;
using kitchenview.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;
using Splat;

namespace kitchenview.DataAccess
{
    public class PhotoprismGalleryDataAccess : IEnableLogger, IDataAccess<PhotoprismImage>
    {
        private readonly IConfiguration configuration;
        
        private readonly RestClient client;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public PhotoprismGalleryDataAccess(IConfiguration configuration, RestClient client)
        {
            this.configuration = configuration;
            this.client = client;
        }

        public Task<IEnumerable<PhotoprismImage>> GetData()
        {
            try
            {
                (bool, string?) tuple = DoLogin();
                if (tuple.Item1)
                {
                    var request = new RestRequest();
                    request.AddHeader("X-Session-Id", tuple.Item2 ?? "");

                    string requestUrl = configuration["Controlls:Gallery:Login"];
                    if (string.IsNullOrEmpty(requestUrl))
                    {
                        this.Log().Error("Configured login url is invalid!");
                        _tokenSource.Cancel();
                        return Task.FromCanceled<IEnumerable<PhotoprismImage>>(_tokenSource.Token);
                    }

                    var response = client.GetAsync(request);
                    if (response?.Result?.StatusCode == HttpStatusCode.OK)
                    {
                        return Task.FromResult<IEnumerable<PhotoprismImage>>(ParseData(response?.Result?.Content ?? ""));
                    }
                    else
                    {
                        _tokenSource.Cancel();
                        return Task.FromCanceled<IEnumerable<PhotoprismImage>>(_tokenSource.Token);
                    }
                }
            }
            catch (Exception exp)
            {
                this.Log().Fatal(exp, "Could not load Photoprism Gallery!");
                return Task.FromException<IEnumerable<PhotoprismImage>>(exp);
            }

            _tokenSource.Cancel();
            return Task.FromCanceled<IEnumerable<PhotoprismImage>>(_tokenSource.Token);

        }

        internal IEnumerable<PhotoprismImage> ParseData(string? rawJsonData)
        {
            var returnValue = new List<PhotoprismImage>();

            if (string.IsNullOrEmpty(rawJsonData))
            {
                this.Log().Error("Raw json data={rawJsonData} is invalid", rawJsonData ?? "-");
                return returnValue;
            }

            try
            {
                var array = new JArray(rawJsonData);
                foreach (JObject element in array)
                {
                    returnValue.Add(new PhotoprismImage()
                    {
                        Name = element["Title"]?.Value<string>() ?? "",
                        Url = $"{configuration["Gallery:Photoprism:ImageBasePath"]}{element["FileName"]?.Value<string>() ?? ""}",
                        Date = DateTime.Parse(element["TakenAt"]?.Value<string>() ?? ""),
                        Comment = element["Description"]?.Value<string>() ?? ""
                    });
                }
            }
            catch (Exception exp)
            {
                this.Log().Fatal(exp, "Could not parse Data! Cannot proceed!");
            }

            return returnValue;
        }

        internal (bool, string?) DoLogin()
        {
            string? loginCredentials = GetLoginCredentials();
            if (string.IsNullOrEmpty(loginCredentials))
            {
                this.Log().Error("Received login data is invalid!");
                return (false, null);
            }

            string requestUrl = configuration["Controlls:Gallery:Login"];
            if (string.IsNullOrEmpty(requestUrl))
            {
                this.Log().Error("Configured login url is invalid!");
                return (false, null);
            }

            try
            {
                var request = new RestRequest(requestUrl);
                request.AddJsonBody(loginCredentials);
                var response = client?.PostAsync(request);
                if (response?.Result?.StatusCode == HttpStatusCode.OK ||
                response?.Result?.StatusCode == HttpStatusCode.Created)
                {
                    return (true, response?.Result?.Content);
                }
                else
                {
                    this.Log().Error("Could not log in received statuscode={statuscode} with message={message}", Convert.ToInt32(response?.Result.StatusCode), response?.Result?.Content ?? "");
                    return (false, null);
                }
            }
            catch (Exception exp)
            {
                this.Log().Fatal(exp, "Error while trying to log into gallery");
                return (false, null);
            }
        }

        internal string? GetLoginCredentials()
        {
            try
            {
                string rawData = configuration["Controlls:Gallery:Login-Body"] ?? "";
                if (string.IsNullOrEmpty(rawData))
                {
                    this.Log().Error("Login-Body in appsettings.json is empty, cannot proceed.");
                    return null;
                }

                string parsedData = Encoding.UTF8.GetString(Convert.FromBase64String(rawData));
                return parsedData;
            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Could not parse login credentials. Invalid Base64 string!");
                return null;
            }
        }
    }
}