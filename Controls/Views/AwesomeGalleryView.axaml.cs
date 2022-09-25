using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using kitchenview.Helper.Transitions;

namespace kitchenview.Controls.Views
{
    public partial class AwesomeGalleryView : UserControl
    {
        private readonly Carousel _carousel;

        private readonly Timer _timer;

        private ICollection<IPageTransition> _transitions;

        public AwesomeGalleryView()
        {
            InitializeComponent();

            InitializeTransitions();

            _carousel = this.FindControl<Carousel>("carousel");
            _carousel.AutoScrollToSelectedItem = false;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += (sender, e) =>
            {
                if (_carousel.SelectedIndex == (_carousel.ItemCount - 1))
                {
                    _carousel.SelectedIndex = 0;
                }
                else
                {
                    _carousel.Next();
                }
                _carousel.PageTransition = _transitions.ElementAt(new Random().Next(0, _transitions.Count() - 1));
            };
            timer.Start();
        }

        internal void InitializeTransitions()
        {
            _transitions = new List<IPageTransition>();
            _transitions.Add(new ShrinkAndGrow(TimeSpan.FromMilliseconds(250)));
            _transitions.Add(new ContinousTransition(TimeSpan.FromMilliseconds(250), PageSlide.SlideAxis.Horizontal));
            _transitions.Add(new CrossFade(TimeSpan.FromSeconds(1)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}