using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Proxies;
using kitchenview.Models;
using Microsoft.Extensions.Configuration;
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
    public class IcsCalendarDataAccess : IEnableLogger, IDataAccess<Appointment>
    {
        private readonly IConfiguration configuration;

        private readonly HttpClient client;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public IcsCalendarDataAccess(IConfiguration configuration, HttpClient client)
        {
            this.configuration = configuration;
            this.client = client;
        }

        public Task<IEnumerable<Appointment>> GetData()
        {
            _tokenSource.Cancel();
            var returnValue = new List<Appointment>();
            try
            {
                var appointments = configuration?.GetSection("Controls:Calendars:ICS:Appointments").Get<IEnumerable<AppointmentConfiguration>>();
                if (appointments is null)
                {
                    this.Log().Error("Invalid appointment configuration. Cannot load Appointments!");
                    return Task.FromCanceled<IEnumerable<Appointment>>(_tokenSource.Token);
                }

                foreach (AppointmentConfiguration config in appointments)
                {
                    var calendar = Calendar.Load(CallIcsData(config.Url).Result);
                    returnValue.AddRange(ConvertEventsToAppointments(calendar.Events, config.ColorCode)?.Result ?? new List<Appointment>(9));
                }

                return Task.FromResult<IEnumerable<Appointment>>(returnValue);
            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Error while parsing ICS data into calendar");
                return Task.FromException<IEnumerable<Appointment>>(exp);
            }
        }

        internal Task<string> CallIcsData(string url)
        {
            _tokenSource.Cancel();

            if (string.IsNullOrEmpty(url))
            {
                this.Log().Error("Passed url={url} was empty", url);
                return Task.FromCanceled<string>(_tokenSource.Token);
            }

            try
            {
                var response = client.GetAsync(url!);
                response.Wait();

                if (response?.Result.StatusCode != HttpStatusCode.OK)
                {
                    return Task.FromCanceled<string>(_tokenSource.Token);
                }
                else
                {
                    var content = response?.Result.Content.ReadAsStringAsync();
                    content.Wait();
                    return Task.FromResult<string>(content.Result ?? "");
                }
            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Could not retrieve ICS data");
                return Task.FromException<string>(exp);
            }
        }

        internal Task<IEnumerable<Appointment>> ConvertEventsToAppointments(IUniqueComponentList<CalendarEvent> events, string colorCode)
        {
            var returnValue = new List<Appointment>();
            try
            {
                foreach (CalendarEvent @event in events)
                {
                    if (@event.RecurrenceRules.Any())
                    {
                        returnValue.Add(new Appointment()
                        {
                            Title = @event.Summary,
                            ColorCode = colorCode,
                            IsLongTitle = @event.Summary?.Length > 20,
                            IsRepeatingEventOnly = true,
                            RepeatingPattern = @event.RecurrenceRules.FirstOrDefault()
                        });
                    }
                    else if (@event.Start?.Value.Year == DateTime.Today.Year)
                    {
                        returnValue.Add(new Appointment()
                        {
                            Title = @event.Summary,
                            DateFrom = @event.Start?.Value,
                            DateTo = @event.End?.Value,
                            TimeFrom = @event.Start?.Value.ToLocalTime().TimeOfDay,
                            TimeTo = @event.End?.Value.ToLocalTime().TimeOfDay,
                            Location = new Location()
                            {
                                Longitude = @event.GeographicLocation?.Longitude ?? 0.0d,
                                Latitude = @event.GeographicLocation?.Latitude ?? 0.0d
                            },
                            ColorCode = colorCode,
                            IsLongTitle = @event.Summary?.Length > 20
                        });
                    }
                }

                return Task.FromResult<IEnumerable<Appointment>>(returnValue);
            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Could not parse calendar event into appointment");
                return Task.FromException<IEnumerable<Appointment>>(exp);
            }
        }
    }
}