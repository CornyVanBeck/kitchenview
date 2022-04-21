using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using kitchenview.Models;
using kitchenview.ViewModels;
using System.Globalization;
using kitchenview.Controls.Views;
using kitchenview.DataAccess;
using Splat;
using Microsoft.Extensions.Configuration;
using System.Timers;
using Newtonsoft.Json.Linq;

namespace kitchenview.Controls.ViewModels
{
    public class AwesomeCalendarViewModel : ViewModelBase
    {
        private readonly IConfiguration configuration;

        private readonly IDataAccess<Appointment> icsData;

        private readonly Timer appointmentInterval;

        private ICollection<Month> Year;

        public ObservableCollection<Week> Weeks
        {
            get;
        }

        public ObservableCollection<Appointment>? Appointments
        {
            get; set;
        }

        public string? CurrentMonth
        {
            get; set;
        }

        public AwesomeCalendarViewModel(IConfiguration configuration, IDataAccess<Appointment> dataAccess)
        {
            this.configuration = configuration;
            this.icsData = dataAccess;
            LoadAppointments();

            appointmentInterval = new Timer();
            appointmentInterval.Interval = TimeSpan.Parse(configuration["Controls:Calendars:ICS:Interval"]).TotalSeconds * 1000;
            appointmentInterval.Elapsed += OnUpdateAppointments;
            appointmentInterval.Start();

            InitializeYear();

            if (Year is null)
            {
                throw new ArgumentNullException("Year", "Year was not properly initiated");
            }

            int currentMonth = DateTime.Now.Month - 1;
            CurrentMonth = Year.ElementAt(currentMonth)?.Name ?? "";
            Weeks = new ObservableCollection<Week>(Year!.ElementAt(currentMonth).Weeks!);
        }

        internal void InitializeYear()
        {
            Year = new List<Month>();
            for (int i = 1; i <= 12; i++)
            {
                Year.Add(new Month()
                {
                    Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
                    Header = null,
                    Value = i,
                    Weeks = GenerateWeeksForMonth(i)
                });
            }
        }

        internal IEnumerable<Week> GenerateWeeksForMonth(int month)
        {
            var returnValue = new List<Week>();
            var calendar = CultureInfo.CurrentCulture.Calendar;
            var daysMap = GetDaysOfMonth(month);
            int weeks = daysMap.Keys.Count();
            DateTime day;
            for (int i = 0; i < weeks; i++)
            {
                day = new DateTime(DateTime.Today.Year, month, daysMap[i].First().DayOfMonth);

                returnValue.Add(new Week()
                {
                    WeekInMonth = i,
                    CalendarWeek = calendar.GetWeekOfYear(day, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
                    Days = new List<Day>(),
                    IsCurrent = daysMap[i].Any(entry => entry.DayOfMonth == DateTime.Today.Day)
                });

                if (i == 0)
                {
                    (returnValue.Last().Days as List<Day>)!.AddRange(AddPreceedingDays(month - 1, day));
                }

                (returnValue.Last().Days as List<Day>)!.AddRange(daysMap[i]);

                if (i == weeks - 1)
                {
                    (returnValue.Last().Days as List<Day>)!.AddRange(AddSucceedingDays(month));
                }
            }
            return returnValue;
        }

        internal IEnumerable<Day> AddSucceedingDays(int month)
        {
            var returnValue = new List<Day>();
            month = month > 12 ? 1 : 12;
            int daysOfMonth = DateTime.DaysInMonth(DateTime.Today.Year, month);
            var day = new DateTime(DateTime.Today.Year, month, daysOfMonth);
            int counter = 1;
            for (int i = Convert.ToInt32(day.DayOfWeek); i <= 6; i++)
            {
                var dayOfWeek = new DateTime(DateTime.Today.Year, month, counter).DayOfWeek;
                int dayOfWeekNumber = Convert.ToInt32(dayOfWeek) - 1;
                dayOfWeekNumber = dayOfWeekNumber < 0 ? 6 : dayOfWeekNumber;

                returnValue.Add(new Day()
                {
                    DayOfMonth = counter,
                    DayOfWeek = dayOfWeekNumber,
                    Name = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dayOfWeek),
                    IsCurrent = counter == DateTime.Today.Day,
                    HasBirthday = false,
                    Appointments = GetCorrespondingAppointments(month, counter),
                    IsEnabled = false
                });

                counter++;
            }

            return returnValue;
        }

        internal IEnumerable<Day> AddPreceedingDays(int month, DateTime day)
        {
            month = month == 0 ? 12 : month;
            var returnValue = new List<Day>();
            int daysOfPreviousMonth = DateTime.DaysInMonth(DateTime.Today.Year, month);
            int countOfPreviousDays = Convert.ToInt32(day.DayOfWeek) - 1;
            int start = daysOfPreviousMonth - countOfPreviousDays + 1;
            for (int i = start; i <= daysOfPreviousMonth; i++)
            {
                var dayOfWeek = new DateTime(DateTime.Today.Year, month, i).DayOfWeek;
                int dayOfWeekNumber = Convert.ToInt32(dayOfWeek) - 1;
                dayOfWeekNumber = dayOfWeekNumber < 0 ? 6 : dayOfWeekNumber;

                returnValue.Add(new Day()
                {
                    DayOfMonth = i,
                    DayOfWeek = dayOfWeekNumber,
                    Name = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dayOfWeek),
                    IsCurrent = i == DateTime.Today.Day,
                    HasBirthday = false,
                    Appointments = GetCorrespondingAppointments(month, i),
                    IsEnabled = false
                });
            }

            return returnValue;
        }

        internal IDictionary<int, IEnumerable<Day>> GetDaysOfMonth(int month)
        {
            var returnValue = new Dictionary<int, IEnumerable<Day>>();
            int weekInMonth = 0;
            for (int i = 1; i <= DateTime.DaysInMonth(DateTime.Today.Year, month); i++)
            {
                var dayOfWeek = new DateTime(DateTime.Today.Year, month, i).DayOfWeek;
                int dayOfWeekNumber = Convert.ToInt32(dayOfWeek) - 1;
                dayOfWeekNumber = dayOfWeekNumber < 0 ? 6 : dayOfWeekNumber;

                if (!returnValue.ContainsKey(weekInMonth))
                {
                    returnValue.Add(weekInMonth, new List<Day>());
                }

                (returnValue[weekInMonth] as List<Day>)!.Add(new Day()
                {
                    DayOfMonth = i,
                    DayOfWeek = dayOfWeekNumber,
                    Name = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dayOfWeek),
                    IsCurrent = i == DateTime.Today.Day,
                    HasBirthday = false,
                    Appointments = GetCorrespondingAppointments(month, i),
                    IsEnabled = true
                });

                if (dayOfWeekNumber == 6)
                {
                    weekInMonth++;
                }
            }

            return returnValue;
        }

        internal IEnumerable<Appointment> GetCorrespondingAppointments(int month, int day)
        {
            var returnValue = new List<Appointment>();

            try
            {
                returnValue = Appointments?.Where(appointment => appointment.DateFrom?.Month == month && 
                                                    appointment.DateFrom?.Day == day &&
                                                    appointment.DateFrom?.Year == DateTime.Today.Year)
                                                    .ToList();
            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Error while trying to load appointemnts for month={month} and day={day}", month, day);
            }

            return returnValue!;
        }

        internal void OnUpdateAppointments(object? sender, EventArgs? args)
        {
            LoadAppointments();
        }

        internal void LoadAppointments()
        {
            var parsedAppointments = icsData?.GetData().Result ?? new List<Appointment>();
            Appointments = new ObservableCollection<Appointment>(parsedAppointments!);
        }
    }
}