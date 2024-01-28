using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private bool _enableAfterWord;

        private bool _enableBeforeWord;

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
                    /* foreach (var definition in wordClockConfig.Definitions)
                    {
                        foreach (var word in definition.Words)
                        {
                            if (longestWord < word.Word.Length)
                            {
                                longestWord = word.Word.Length;
                            }
                        }
                    } */

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

            _timer.Interval = TimeSpan.FromSeconds(30);
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            foreach (var definition in ListOfDefinitions)
            {
                foreach (var word in definition.Words)
                {
                    if (definition.Index >= 5)
                    {
                        var hourToCheck = DateTime.Now.Hour > 12 ? DateTime.Now.Hour - 12 : DateTime.Now.Hour;
                        var minuteToCheck = Convert.ToInt32(DateTime.Now.Minute / 10) * 10;
                        if (minuteToCheck >= 30)
                        {
                            hourToCheck++;
                        }
                        word.IsEnabled = word.Special == "ALWAYS_ON" ? true : word.Value == hourToCheck ? true : false;
                    }
                    else if (definition.Index == 4)
                    {
                        var hourToCheck = DateTime.Now.Hour > 12 ? DateTime.Now.Hour - 12 : DateTime.Now.Hour;
                        var minuteToCheck = Convert.ToInt32(DateTime.Now.Minute / 10) * 10;
                        if (minuteToCheck >= 30)
                        {
                            word.IsEnabled = word.Value == minuteToCheck ? true : false;
                            hourToCheck++;
                            word.IsEnabled = word.Value == hourToCheck ? true : false;
                            continue;
                        }
                        word.IsEnabled = word.Special == "ALWAYS_ON" ? true : word.Value == hourToCheck ? true : false;
                    }
                    else
                    {
                        var minuteToCheck = DateTime.Now.Minute > 10 ? Convert.ToInt32(DateTime.Now.Minute / 10) * 10 : DateTime.Now.Minute;
                        if (DateTime.Now.Minute >= 15 && DateTime.Now.Minute < 19)
                        {
                            minuteToCheck = 15;
                        }

                        if (DateTime.Now.Minute >= 45 && DateTime.Now.Minute < 49)
                        {
                            minuteToCheck = 45;
                        }

                        var minutesUntilToCheck = 60 - minuteToCheck;
                        var isSpecial = word.Special == SpecialType.ALWAYS_ON.ToString("g") ||
                                        word.Special == SpecialType.HOUR_WORD.ToString("g");
                        word.IsEnabled = isSpecial ?
                                        true : word.Value == minuteToCheck ?
                                        EnableAfterWord() : word.Value == minutesUntilToCheck ?
                                        EnableBeforeWord() : false;
                    }
                }
            }

            if (_enableAfterWord)
            {
                foreach (var definition in ListOfDefinitions)
                {
                    foreach (var word in definition.Words)
                    {
                        if (word.Special == "AFTER")
                        {
                            word.IsEnabled = true;
                            break;
                        }
                    }
                }
            }

            if (_enableBeforeWord)
            {
                foreach (var definition in ListOfDefinitions)
                {
                    foreach (var word in definition.Words)
                    {
                        if (word.Special == "BEFORE")
                        {
                            word.IsEnabled = true;
                            break;
                        }
                    }
                }
            }

            foreach (var definition in ListOfDefinitions)
            {
                foreach (var word in definition.Words)
                {
                    if (word.Special == SpecialType.HOUR_WORD.ToString("g"))
                    {
                        word.IsEnabled = false;
                        if (DateTime.Now.Minute >= 0 && DateTime.Now.Minute < 4)
                        {
                            word.IsEnabled = true;
                        }
                        break;
                    }
                }
            }

            Definitions = new ObservableCollection<WordClockConfigDefinition>(ListOfDefinitions);
        }

        internal bool EnableAfterWord()
        {
            _enableAfterWord = true;
            _enableBeforeWord = false;
            return true;
        }

        internal bool EnableBeforeWord()
        {
            _enableAfterWord = false;
            _enableBeforeWord = true;
            return true;
        }
    }
}