using kitchenview.DataAccess;
using kitchenview.Helper.Comparer;
using kitchenview.Helper.Extensions;
using kitchenview.Models;
using kitchenview.ViewModels;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Timers;

namespace kitchenview.Controls.ViewModels
{
    public class AwesomeCalendarViewModel : ViewModelBase
    {
        private readonly IConfiguration configuration;

        private readonly IDataAccess<Appointment> icsData;

        private readonly Timer appointmentInterval;

        private ICollection<Month> Year;

        public ObservableCollection<Day> WeekDays
        {
            get;
        }

        public ObservableCollection<Week> Weeks
        {
            get;
        }

        public ObservableCollection<Appointment> Appointments
        {
            get;
        }

        public string? CurrentMonth
        {
            get; set;
        }

        public AwesomeCalendarViewModel(IConfiguration configuration, IDataAccess<Appointment> dataAccess)
        {
            this.configuration = configuration;
            this.icsData = dataAccess;

            Appointments = new ObservableCollection<Appointment>();

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
            month = month > 12 ? 1 : month;
            int daysOfMonth = DateTime.DaysInMonth(DateTime.Today.Year, month);
            var day = new DateTime(DateTime.Today.Year, month, daysOfMonth);
            int counter = 1;
            for (int i = Convert.ToInt32(day.DayOfWeek); i < 7; i++)
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
                    Appointments = GetCorrespondingAppointments(month + 1, counter),
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
            int numericDayOfWeek = Convert.ToInt32(day.DayOfWeek);
            numericDayOfWeek = numericDayOfWeek == 0 ? 7 : numericDayOfWeek;
            int countOfPreviousDays = numericDayOfWeek - 1;
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

        internal ObservableCollection<Appointment> GetCorrespondingAppointments(int month, int day)
        {
            var returnValue = new ObservableCollection<Appointment>();

            try
            {
                returnValue = new ObservableCollection<Appointment>(Appointments?.Where(appointment => appointment.DateFrom?.Month == month &&
                                                                    appointment.DateFrom?.Day == day &&
                                                                    appointment.DateFrom?.Year == DateTime.Today.Year)!);
            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Error while trying to load appointemnts for month={month} and day={day}", month, day);
            }

            return returnValue!;
        }

        internal void OnUpdateAppointments(object? sender, EventArgs? args)
        {
            Debug.WriteLine("Updating appointments");
            LoadAppointments();
        }

        internal void LoadAppointments()
        {
            var parsedAppointments = icsData?.GetData().Result ?? new List<Appointment>();
            var newData = new ObservableCollection<Appointment>(parsedAppointments!);
            var filteredAppointments = newData.Where(entry => entry.DateFrom?.Month == DateTime.Today.Month);
            if (Weeks is null)
            {
                foreach (Appointment item in filteredAppointments)
                {
                    var foundItem = Appointments.FirstOrDefault(entry => entry.Title == item.Title &&
                                                                        entry.DateFrom == item.DateFrom &&
                                                                        entry.DateTo == item.DateTo &&
                                                                        entry.TimeFrom == item.TimeFrom &&
                                                                        entry.TimeTo == item.TimeTo);
                    if (foundItem is not null)
                    {
                        foundItem.ColorCode += $";{item.ColorCode}";
                    }
                    else
                    {
                        Appointments.Add(item);
                    }
                }
                return;
            }


            ObservableCollection<Appointment> difference;
#if DEBUG
            Console.WriteLine($"Appointments: {Appointments.Count} <> Filtered Appointments: {filteredAppointments.Count()}");
#endif
            if (filteredAppointments.Count() > Appointments.Count)
            {
                difference = new ObservableCollection<Appointment>(filteredAppointments.Except(Appointments, new AppointmentComparer()));
#if DEBUG
                Console.WriteLine("Adding new ones");
#endif
                AddNewAppointments(difference);
            }
            else
            {
                difference = new ObservableCollection<Appointment>(Appointments.Except(filteredAppointments, new AppointmentComparer()));
#if DEBUG
                Console.WriteLine("Removing old ones");
#endif
                RemoveOldAppointments(difference);
            }
        }

        internal void AddNewAppointments(ObservableCollection<Appointment> difference)
        {
            foreach (Week week in Weeks)
            {
                foreach (Day day in week.Days!)
                {
                    foreach (Appointment item in difference)
                    {
                        if (day?.DayOfMonth == item.DateFrom?.Day)
                        {
                            if (day!.Appointments is null)
                            {
                                day.Appointments = new ObservableCollection<Appointment>();
                            }

                            if (day!.Appointments!.Contains(item, new AppointmentComparer()))
                            {
                                break;
                            }
                            else
                            {
                                day!.Appointments!.Add(item!);
                                Appointments.Add(item!);
                            }
                        }
                    }
                }
            }
        }

        internal void RemoveOldAppointments(ObservableCollection<Appointment> difference)
        {
            foreach (Week week in Weeks)
            {
                foreach (Day day in week.Days!)
                {
                    foreach (Appointment item in difference)
                    {
                        if (day?.DayOfMonth == item.DateFrom?.Day)
                        {
                            if (day!.Appointments is null)
                            {
                                break;
                            }

                            int indexOf = day!.Appointments!.ItemAt<Appointment>(item, new AppointmentComparer());
                            if (indexOf >= 0)
                            {
                                day!.Appointments!.RemoveAt(indexOf);
                                Appointments.RemoveAt(indexOf);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}