using System;
using System.Collections.ObjectModel;
using System.Timers;
using kitchenview.DataAccess;
using kitchenview.Models;
using Microsoft.Extensions.Configuration;
using System.Linq;
using kitchenview.ViewModels;

namespace kitchenview.Controls.ViewModels
{
    public class AwesomeShoppingListViewModel : ViewModelBase
    {
        private readonly IConfiguration configuration;

        private readonly IDataAccess<TodoistShoppingListEntry> dataAccess;

        public ObservableCollection<IShoppingListEntry> ShoppingList { get; set; }

        public AwesomeShoppingListViewModel(IConfiguration configuration, IDataAccess<TodoistShoppingListEntry> dataAccess)
        {
            this.configuration = configuration;
            this.dataAccess = dataAccess;

            ShoppingList = new ObservableCollection<IShoppingListEntry>();
            var timer = new Timer();
            timer.Interval = TimeSpan.Parse(configuration?["Controls:Todoist:Interval"]?.ToString() ?? "00:05:00").TotalMilliseconds;
            timer.Elapsed += OnTick;
            timer.Start();
            LoadShoppingList();
        }

        internal void OnTick(object sender, EventArgs args)
        {
            LoadShoppingList();
        }

        internal async void LoadShoppingList()
        {
            var response = dataAccess.GetData();
            response.Wait();

            if (!response?.Result?.Any() ?? true)
            {
                ShoppingList?.Clear();
            }
            else
            {
                foreach (IShoppingListEntry entry in response?.Result)
                {
                    if (!ShoppingList.Any(item => item.Name == entry.Name))
                    {
                        ShoppingList.Add(entry as TodoistShoppingListEntry);
                    }
                }

                foreach (IShoppingListEntry entryToRemove in ShoppingList)
                {
                    if (!response.Result.Any(item => item.Name == entryToRemove.Name))
                    {
                        ShoppingList.Remove(entryToRemove);
                    }
                }
            }
        }
    }
}