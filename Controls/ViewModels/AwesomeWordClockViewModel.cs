using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Avalonia.Threading;
using DynamicData.Binding;
using kitchenview.Models;
using kitchenview.ViewModels;
using Microsoft.Extensions.Configuration;
using ReactiveUI;

namespace kitchenview.Controls.ViewModels
{
    public class AwesomeWordClockViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private int _hourToAdd = 0;

        private int _fastForwardMinute = 0;

        private int _fastForwardHour = DateTime.Now.Hour > 12 ? DateTime.Now.Hour - 12 : DateTime.Now.Hour;

        private SpecialType _beforeAndAfterState;

        public List<WordClockConfigDefinition> ListOfDefinitions
        {
            get; set;
        }

        public ObservableCollection<WordClockConfigDefinition> _definitions;

        public ObservableCollection<WordClockConfigDefinition> Definitions
        {
            get => _definitions;
            private set => this.RaiseAndSetIfChanged(ref _definitions, value);
        }

        public AwesomeWordClockViewModel(IConfiguration configuration)
        {
            var wordClockConfig = configuration?.GetSection("Controls:WordClock").Get<WordClockConfig>();
            if (wordClockConfig is not null)
            {
                if (wordClockConfig.SpaceFiller == SpaceFillerType.RANDOM_LETTER.ToString("g"))
                {
                    var longestWord = 12;

                    ListOfDefinitions = new List<WordClockConfigDefinition>();
                    foreach (var definition in wordClockConfig.Definitions)
                    {
                        var generatedWords = new List<WordClockConfigDefinitionWord>();
                        if (definition.Words.Sum(entry => entry.Word.Length) == longestWord)
                        {
                            generatedWords.AddRange(definition.Words);
                            definition.Words = generatedWords;
                            ListOfDefinitions.Add(definition);
                            continue;
                        }

                        var wordCounter = 0;
                        var currentLength = generatedWords.Sum(entry => entry.Word.Length);
                        var delta = longestWord - definition.Words.Sum(entry => entry.Word.Length);
                        while (currentLength < longestWord)
                        {
                            generatedWords.Add(definition.Words.ElementAt(wordCounter));
                            currentLength = generatedWords.Sum(entry => entry.Word.Length);

                            if (wordCounter < definition.Words.Count() - 1)
                            {
                                wordCounter++;
                            }

                            currentLength = generatedWords.Sum(entry => entry.Word.Length);
                            if (currentLength + definition.Words.ElementAt(wordCounter).Word.Length >= longestWord)
                            {
                                var lengthForLastLetters = longestWord - currentLength;
                                for (var i = 0; i < lengthForLastLetters; i++)
                                {
                                    generatedWords.Add(new WordClockConfigDefinitionWord()
                                    {
                                        IsEnabled = false,
                                        Word = Convert.ToChar(new Random().Next(65, 90)).ToString()
                                    });
                                }
                                break;
                            }
                            else
                            {
                                for (var i = 0; i < delta / 2; i++)
                                {
                                    generatedWords.Add(new WordClockConfigDefinitionWord()
                                    {
                                        IsEnabled = false,
                                        Word = Convert.ToChar(new Random().Next(65, 90)).ToString()
                                    });
                                }
                            }
                        }

                        definition.Words = generatedWords;
                        ListOfDefinitions.Add(definition);
                    }
                }
                ListOfDefinitions = new List<WordClockConfigDefinition>(wordClockConfig.Definitions);
                Definitions = new ObservableCollection<WordClockConfigDefinition>(ListOfDefinitions);
                UpdateClock();
            }

#if DEBUG
            _timer.Interval = TimeSpan.FromMilliseconds(500);
#else
            _timer.Interval = TimeSpan.FromSeconds(5);
#endif
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
#if DEBUG
            if (_fastForwardHour == 12)
            {
                _fastForwardHour = 0;
            }

            if (_fastForwardMinute == 60)
            {
                _fastForwardHour++;
                _fastForwardMinute = 0;
            }
            _fastForwardMinute++;
            Debug.WriteLine(_fastForwardHour.ToString("00") + ":" + _fastForwardMinute.ToString("00"));
#endif
            _ = CurrentMinute();
            _ = CurrentHour();
            var alwaysOn = ListOfDefinitions.SelectMany(entry => entry.Words)
                                            .Where(entry => entry.Special == SpecialType.ALWAYS_ON.ToString("g"));
            foreach (var word in alwaysOn)
            {
                word.IsEnabled = true;
            }

            var beforeAndAfter = ListOfDefinitions.SelectMany(entry => entry.Words)
                                            .Where(entry => entry.Special == SpecialType.BEFORE.ToString("g") ||
                                                            entry.Special == SpecialType.AFTER.ToString("g"));
            foreach (var word in beforeAndAfter)
            {
                word.IsEnabled = false;
                if (_beforeAndAfterState == SpecialType.AFTER &&
                word.Special == SpecialType.AFTER.ToString("g"))
                {
                    word.IsEnabled = true;
                }

                if (_beforeAndAfterState == SpecialType.BEFORE &&
                word.Special == SpecialType.BEFORE.ToString("g"))
                {
                    word.IsEnabled = true;
                }
            }

            var oClock = ListOfDefinitions.SelectMany(entry => entry.Words)
                                        .Where(entry => entry.Special == SpecialType.HOUR_WORD.ToString("g"))
                                        .FirstOrDefault();
            if (CurrentMinute() == 0 && oClock is not null)
            {
                oClock.IsEnabled = true;
            }
            else if (oClock is not null)
            {
                oClock.IsEnabled = false;
            }

            EnableMinutes(ListOfDefinitions.SelectMany(entry => entry.Words)
                                            .Where(entry => entry.Type == "MINUTE"));

            EnableHour(ListOfDefinitions.SelectMany(entry => entry.Words)
                                            .Where(entry => entry.Type == "HOUR")
                                            );

            Definitions = new ObservableCollection<WordClockConfigDefinition>(ListOfDefinitions);
        }

        internal void EnableHour(IEnumerable<WordClockConfigDefinitionWord> words)
        {
            foreach (var word in words)
            {
                word.IsEnabled = false;
                var hourToCheck = CurrentHour() + _hourToAdd;
                if (word.Value == (hourToCheck> 12 ? hourToCheck - 12 : hourToCheck))
                {
                    word.IsEnabled = true;
                }
            }
        }

        internal bool EnableMinutes(IEnumerable<WordClockConfigDefinitionWord> words)
        {
            foreach (var word in words)
            {
                word.IsEnabled = false;
                if (word.Value == CurrentMinute())
                {
                    word.IsEnabled = true;
                }
            }

            return false;
        }

        internal int CurrentMinute()
        {
            _beforeAndAfterState = SpecialType.NONE;
            _hourToAdd = 0;
            var minute = DateTime.Now.Minute;
#if DEBUG
            minute = _fastForwardMinute;
#endif
            var minuteToCheck = Convert.ToInt32(minute / 10) * 10;
            if (minute >= 5 && minuteToCheck < 30)
            {
                _beforeAndAfterState = SpecialType.AFTER;
            }
            else if (minuteToCheck == 30)
            {
                _beforeAndAfterState = SpecialType.NONE;
                _hourToAdd = 1;
            }
            else if (minute >= 40)
            {
                minuteToCheck = 60 - minuteToCheck;
                _beforeAndAfterState = SpecialType.BEFORE;
                _hourToAdd = 1;
            }

            if ((minute >= 5 && minute < 10) ||
            (minute >= 55 && minute < 60))
            {
                return 5;
            }

            if ((minute >= 15 && minute < 20) ||
            (minute >= 45 && minute < 50))
            {
                return 15;
            }

            return minuteToCheck;
        }

        internal int CurrentHour()
        {
            var hourToCheck = DateTime.Now.Hour > 12 ? DateTime.Now.Hour - 12 : DateTime.Now.Hour;
#if DEBUG
            hourToCheck = _fastForwardHour;
#endif
            return hourToCheck == 0 ? 12 : hourToCheck;
        }
    }
}