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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
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
                    string requestUrl = configuration["Controls:Gallery:Url"];
                    if (string.IsNullOrEmpty(requestUrl))
                    {
                        this.Log().Error("Configured login url is invalid!");
                        _tokenSource.Cancel();
                        return Task.FromCanceled<IEnumerable<PhotoprismImage>>(_tokenSource.Token);
                    }

                    var request = new RestRequest(requestUrl);
                    request.AddHeader("X-Session-Id", tuple.Item2 ?? "");
                    var response = client.GetAsync(request);
                    response.Wait();
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
                var client = CreateRestClientWithBasicAuthentication();
                var array = JArray.Parse(rawJsonData);
                foreach (JToken element in array)
                {
                    returnValue.Add(new PhotoprismImage(client)
                    {
                        Name = element["Title"]?.ToString() ?? "",
                        Url = $"{configuration["Controls:Gallery:Photoprism:BaseUrl"]}" +
                                $"{configuration["Controls:Gallery:Photoprism:ImageBasePath"]}/" +
                                $"{(element["FileName"]?.ToString() ?? "")}",
                        Date = DateTime.Parse(element["TakenAt"]?.ToString() ?? ""),
                        Comment = element["Description"]?.ToString() ?? ""
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

            string requestUrl = configuration["Controls:Gallery:Login"];
            if (string.IsNullOrEmpty(requestUrl))
            {
                this.Log().Error("Configured login url is invalid!");
                return (false, null);
            }

            try
            {
                var request = new RestRequest(requestUrl);
                request.AddStringBody(loginCredentials, DataFormat.Json);
                var response = client?.PostAsync(request);
                response?.Wait();
                if (response?.Result?.StatusCode == HttpStatusCode.OK ||
                response?.Result?.StatusCode == HttpStatusCode.Created)
                {
                    JObject parsedContent = JObject.Parse(response?.Result?.Content ?? "");
                    if (parsedContent?.ContainsKey("id") ?? false)
                    {
                        return (true, parsedContent?["id"]?.ToString());
                    }
                    else
                    {
                        return (false, null);
                    }
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
                string rawData = configuration["Controls:Gallery:Login-Body"] ?? "";
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

        private RestClient CreateRestClientWithBasicAuthentication()
        {
            JObject credentials = JObject.Parse(GetLoginCredentials() ?? "");
            var client = new RestClient();
            if (credentials is not null &&
                credentials.ContainsKey("username") &&
                credentials["username"] is not null &&
                credentials.ContainsKey("password") &&
                credentials["password"] is not null)
            {
                client.Authenticator = new HttpBasicAuthenticator(credentials!["username"]!.ToString(), credentials!["password"]!.ToString());
            }
            else
            {
                this.Log().Warn("Could not add Basic Authentication");
            }

            return client;
        }
    }
}