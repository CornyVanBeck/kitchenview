using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using kitchenview.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using Splat;

namespace kitchenview.DataAccess
{
    public class QuoteDataAccess : IDataAccess<IQuote>, IEnableLogger
    {
        private readonly IConfiguration configuration;

        private readonly RestClient client;

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        public QuoteDataAccess(IConfiguration configuration, RestClient client)
        {
            this.client = client;
            this.configuration = configuration;
        }

        public Task<IEnumerable<IQuote>> GetData()
        {
            tokenSource.Cancel();
            string url = configuration["Controls:Quotes:Endpoint"];
            if (string.IsNullOrEmpty(url))
            {
                this.Log().Error("Configured url={url} is invalid", url);
                return Task.FromCanceled<IEnumerable<IQuote>>(tokenSource.Token);
            }

            string authToken = configuration["Controls:Quotes:ApiKey"];
            if (string.IsNullOrEmpty(authToken))
            {
                this.Log().Error("Configuration authorization token={authToken} is invalid", authToken);
                return Task.FromCanceled<IEnumerable<IQuote>>(tokenSource.Token);
            }

            try
            {
                var request = new RestRequest(url);
                request.AddHeader("Authorization", $"Token {authToken}");
                var response = client?.GetAsync(request).Result;

                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    if (string.IsNullOrEmpty(response.Content))
                    {
                        this.Log().Error("Received empty data instead of proper quote");
                        return Task.FromCanceled<IEnumerable<IQuote>>(tokenSource.Token);
                    }
                    return Task.FromResult<IEnumerable<IQuote>>(new List<IQuote>() { JsonConvert.DeserializeObject<QuoteOfTheDay>(response.Content!) });
                }
                else
                {
                    this.Log().Error("Error while requesting quote: {result}", response?.Content ?? "");
                    return Task.FromCanceled<IEnumerable<IQuote>>(tokenSource.Token);
                }
            }
            catch (Exception exp)
            {
                this.Log().Fatal(exp, "Error while trying to retrieve quote through url={url}", url);
                return Task.FromException<IEnumerable<IQuote>>(exp);
            }
        }
    }
}