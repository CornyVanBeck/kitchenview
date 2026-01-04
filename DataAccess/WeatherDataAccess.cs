using kitchenview.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using openmeteo_sdk;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace kitchenview.DataAccess
{
    public class WeatherDataAccess : IEnableLogger, IDataAccess<Weather>
    {
        private readonly IConfiguration _configuration;

        private readonly HttpClient _client;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public WeatherDataAccess(IConfiguration configuration, HttpClient client)
        {
            _configuration = configuration;
            _client = client;
        }

        public async Task<IEnumerable<Weather>> GetData()
        {
            _tokenSource.Cancel();
            var returnValue = Enumerable.Empty<Weather>();
            try
            {
                var weatherEndpoint = _configuration?.GetSection("Controls:Weather:Endpoint").Get<string>();
                if (weatherEndpoint is null)
                {
                    this.Log().Error("Invalid Weather Endpoint. Cannot load Weather!");
                    return null!;
                }

                var response = _client.GetAsync(weatherEndpoint!);
                response.Wait();

                if (response?.Result.StatusCode != HttpStatusCode.OK)
                {
                    return null!;
                }
                else
                {
                    var content = await response?.Result?.Content?.ReadAsStringAsync();
                    if (content is null)
                        return null!;

                    var meteoData = JsonConvert.DeserializeObject<WeatherApiResponse>(content);
                    return await ConvertEventsToAppointments(meteoData) ?? [];
                }

            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Error while parsing ICS data into calendar");
                return null!;
            }
        }

        internal async Task<IEnumerable<Weather>> ConvertEventsToAppointments(WeatherApiResponse meteoData)
        {
            var returnValue = new List<Weather>();
            try
            {
                return returnValue;
            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Could not parse weather api response into weather data");
                return null!;
            }
        }
    }
}