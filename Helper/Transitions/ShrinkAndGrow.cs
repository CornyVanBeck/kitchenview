using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace kitchenview.Helper.Transitions
{
    public class ShrinkAndGrow : IPageTransition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShrinkAndGrow"/> class.
        /// </summary>
        public ShrinkAndGrow()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShrinkAndGrow"/> class.
        /// </summary>
        /// <param name="duration">The duration of the animation.</param>
        public ShrinkAndGrow(TimeSpan duration)
        {
            Duration = duration;
        }

        /// <summary>
        /// Gets the duration of the animation.
        /// </summary>
        public TimeSpan Duration { get; set; }

        public async Task Start(Visual from, Visual to, bool forward, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var tasks = new List<Task>();
            var parent = GetVisualParent(from, to);
            var scaleYProperty = ScaleTransform.ScaleYProperty;

            if (from != null)
            {
                var animation = new Animation
                {
                    FillMode = FillMode.Forward,
                    Children =
                {
                    new KeyFrame
                    {
                        Setters = {
                            new Setter { Property = scaleYProperty, Value = 1d }
                            },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = scaleYProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                    Duration = Duration
                };
                tasks.Add(animation.RunAsync(from, null, cancellationToken));
            }

            if (to != null)
            {
                to.IsVisible = true;
                var animation = new Animation
                {
                    FillMode = FillMode.Forward,
                    Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = scaleYProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters = { new Setter { Property = scaleYProperty, Value = 1d } },
                        Cue = new Cue(1d)
                    }
                },
                    Duration = Duration
                };
                tasks.Add(animation.RunAsync(to, null, cancellationToken));
            }

            await Task.WhenAll(tasks);

            if (from != null && !cancellationToken.IsCancellationRequested)
            {
                from.IsVisible = false;
            }
        }

        /// <summary>
        /// Gets the common visual parent of the two control.
        /// </summary>
        /// <param name="from">The from control.</param>
        /// <param name="to">The to control.</param>
        /// <returns>The common parent.</returns>
        /// <exception cref="ArgumentException">
        /// The two controls do not share a common parent.
        /// </exception>
        /// <remarks>
        /// Any one of the parameters may be null, but not both.
        /// </remarks>
        private static IVisual GetVisualParent(IVisual? from, IVisual? to)
        {
            var p1 = (from ?? to)!.VisualParent;
            var p2 = (to ?? from)!.VisualParent;

            if (p1 != null && p2 != null && p1 != p2)
            {
                throw new ArgumentException("Controls for PageSlide must have same parent.");
            }

            return p1 ?? throw new InvalidOperationException("Cannot determine visual parent.");
        }
    }
}