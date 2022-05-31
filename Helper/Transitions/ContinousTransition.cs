using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Avalonia.Animation.PageSlide;

namespace kitchenview.Helper.Transitions
{
    public class ContinousTransition : IPageTransition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContinousTransition"/> class.
        /// </summary>
        public ContinousTransition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinousTransition"/> class.
        /// </summary>
        /// <param name="duration">The duration of the animation.</param>
        public ContinousTransition(TimeSpan duration, SlideAxis orientation = SlideAxis.Horizontal)
        {
            Duration = duration;
            Orientation = orientation;
        }

        /// <summary>
        /// Gets the duration of the animation.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets the orientation of the animation.
        /// </summary>
        public SlideAxis Orientation { get; set; }

        /// <summary>
        /// Gets or sets element entrance easing.
        /// </summary>
        public Easing SlideInEasing { get; set; } = new LinearEasing();

        /// <summary>
        /// Gets or sets element exit easing.
        /// </summary>
        public Easing SlideOutEasing { get; set; } = new LinearEasing();

        public virtual async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Visual? innerFrom = from;
            Visual? innerTo = to;

            var tasks = new List<Task>();
            var parent = GetVisualParent(innerFrom, innerTo);
            var distance = Orientation == SlideAxis.Horizontal ? parent.Bounds.Width : parent.Bounds.Height;
            var translateProperty = Orientation == SlideAxis.Horizontal ? TranslateTransform.XProperty : TranslateTransform.YProperty;

            if (innerFrom != null)
            {
                var animation = new Animation
                {
                    Easing = SlideOutEasing,
                    FillMode = FillMode.Forward,
                    Children =
                    {
                        new KeyFrame
                        {
                            Setters = { new Setter { Property = translateProperty, Value = 0d } },
                            Cue = new Cue(0d)
                        },
                        new KeyFrame
                        {
                            Setters =
                            {
                                new Setter
                                {
                                    Property = translateProperty,
                                    Value = -distance
                                }
                            },
                            Cue = new Cue(1d)
                        }
                    },
                    Duration = Duration
                };
                tasks.Add(animation.RunAsync(innerFrom, null, cancellationToken));
            }

            if (innerTo != null)
            {
                innerTo.IsVisible = true;
                var animation = new Animation
                {
                    FillMode = FillMode.Forward,
                    Easing = SlideInEasing,
                    Children =
                    {
                        new KeyFrame
                        {
                            Setters =
                            {
                                new Setter
                                {
                                    Property = translateProperty,
                                    Value = distance
                                }
                            },
                            Cue = new Cue(0d)
                        },
                        new KeyFrame
                        {
                            Setters = { new Setter { Property = translateProperty, Value = 0d } },
                            Cue = new Cue(1d)
                        }
                    },
                    Duration = Duration
                };
                tasks.Add(animation.RunAsync(innerTo, null, cancellationToken));
            }

            await Task.WhenAll(tasks);

            if (innerFrom != null && !cancellationToken.IsCancellationRequested)
            {
                innerFrom.IsVisible = false;
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