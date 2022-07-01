using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using kitchenview.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using Splat;

namespace kitchenview.DataAccess
{
    public class TodoistDataAccess : IEnableLogger, IDataAccess<TodoistShoppingListEntry>
    {
        private readonly IConfiguration configuration;

        private readonly RestClient client;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public TodoistDataAccess(IConfiguration configuration)
        {
            this.configuration = configuration;

            string apiToken = configuration?["Controls:Todoist:APIToken"]?.ToString() ?? "";
            if (string.IsNullOrEmpty(apiToken))
            {
                this.Log().Error("No API token configured");
                _tokenSource.Cancel();
                return;
            }

            client = new RestClient();
            client.Authenticator = new JwtAuthenticator(apiToken);
        }

        public async Task<IEnumerable<TodoistShoppingListEntry>> GetData()
        {
            var returnValue = new List<TodoistShoppingListEntry>();
            try
            {
                string requestUrl = configuration?["Controls:Todoist:Endpoints:ShoppingList"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(requestUrl))
                {
                    this.Log().Error("Configured url is invalid!");
                    _tokenSource.Cancel();
                    return await Task.FromCanceled<IEnumerable<TodoistShoppingListEntry>>(_tokenSource.Token);
                }

                var request = new RestRequest(requestUrl);
                var response = client?.GetAsync(request);
                response?.Wait();

                if (response is not null &&
                    response.Result.StatusCode == HttpStatusCode.OK)
                {
                    var array = JArray.Parse(response?.Result?.Content ?? "[]");

                    foreach (JToken item in array)
                    {
                        returnValue.Add(new TodoistShoppingListEntry()
                        {
                            Name = item?["content"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception exp)
            {
                return await Task.FromException<IEnumerable<TodoistShoppingListEntry>>(exp);
            }

            return await Task.FromResult<IEnumerable<TodoistShoppingListEntry>>(returnValue);
        }
    }
}