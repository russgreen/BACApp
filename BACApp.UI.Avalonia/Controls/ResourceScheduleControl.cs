using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BACApp.UI.Avalonia.Controls
{
    public class ResourceScheduleControl : TemplatedControl
    {
        public static readonly StyledProperty<IEnumerable<Resource>?> ResourcesProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, IEnumerable<Resource>?>(nameof(Resources));

        public static readonly StyledProperty<IEnumerable<ResourceEvent>?> EventsProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, IEnumerable<ResourceEvent>?>(nameof(Events));

        public static readonly StyledProperty<DateOnly> DayProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, DateOnly>(nameof(Day), DateOnly.FromDateTime(DateTime.Today));

        public static readonly StyledProperty<double> HourWidthProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, double>(nameof(HourWidth), 80);

        public static readonly StyledProperty<double> RowHeightProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, double>(nameof(RowHeight), 36);

        public static readonly StyledProperty<double> HeaderWidthProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, double>(nameof(HeaderWidth), 200);

        public static readonly StyledProperty<IBrush> GridLineBrushProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, IBrush>(nameof(GridLineBrush), Brushes.Gray);

        public static readonly StyledProperty<IBrush> BackgroundBrushProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, IBrush>(nameof(BackgroundBrush), Brushes.Transparent);

        public static readonly StyledProperty<int> StartHourProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, int>(nameof(StartHour), 0);

        public static readonly StyledProperty<int> EndHourProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, int>(nameof(EndHour), 24);

        // Height of the header band where hour labels are drawn.
        public static readonly StyledProperty<double> TimeHeaderHeightProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, double>(nameof(TimeHeaderHeight), 20);

        public IEnumerable<Resource>? Resources
        {
            get => GetValue(ResourcesProperty);
            set => SetValue(ResourcesProperty, value);
        }

        public IEnumerable<ResourceEvent>? Events
        {
            get => GetValue(EventsProperty);
            set => SetValue(EventsProperty, value);
        }

        public DateOnly Day
        {
            get => GetValue(DayProperty);
            set => SetValue(DayProperty, value);
        }

        public double HourWidth
        {
            get => GetValue(HourWidthProperty);
            set => SetValue(HourWidthProperty, value);
        }

        public double RowHeight
        {
            get => GetValue(RowHeightProperty);
            set => SetValue(RowHeightProperty, value);
        }

        public double HeaderWidth
        {
            get => GetValue(HeaderWidthProperty);
            set => SetValue(HeaderWidthProperty, value);
        }

        public IBrush GridLineBrush
        {
            get => GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }

        public IBrush BackgroundBrush
        {
            get => GetValue(BackgroundBrushProperty);
            set => SetValue(BackgroundBrushProperty, value);
        }

        public int StartHour
        {
            get => GetValue(StartHourProperty);
            set => SetValue(StartHourProperty, value);
        }

        public int EndHour
        {
            get => GetValue(EndHourProperty);
            set => SetValue(EndHourProperty, value);
        }

        public double TimeHeaderHeight
        {
            get => GetValue(TimeHeaderHeightProperty);
            set => SetValue(TimeHeaderHeightProperty, value);
        }

        private ScrollViewer? _scrollViewer;
        private ItemsControl? _resourceList;
        private Canvas? _timelineCanvas;
        private Rect _lastBounds;

        public ResourceScheduleControl()
        {
            ResourcesProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.OnDataChanged());
            EventsProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.Redraw());
            DayProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.Redraw());
            HourWidthProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.Redraw());
            RowHeightProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.Redraw());
            HeaderWidthProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.Redraw());
            StartHourProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.Redraw());
            EndHourProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.Redraw());
            TimeHeaderHeightProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.Redraw());

            PropertyChanged += (_, e) =>
            {
                if (e.Property == BoundsProperty)
                {
                    var b = (Rect)e.NewValue!;
                    if (!_lastBounds.Equals(b))
                    {
                        _lastBounds = b;
                        Redraw();
                    }
                }
            };
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
            _resourceList = e.NameScope.Find<ItemsControl>("PART_ResourceList");
            _timelineCanvas = e.NameScope.Find<Canvas>("PART_TimelineCanvas");

            if (_resourceList != null)
            {
                _resourceList.ItemsSource = Resources ?? Enumerable.Empty<Resource>();
            }

            Redraw();
        }

        private void OnDataChanged()
        {
            if (_resourceList != null)
            {
                _resourceList.ItemsSource = Resources ?? Enumerable.Empty<Resource>();
            }

            Redraw();
        }

        private void Redraw()
        {
            if (_timelineCanvas == null)
            {
                return;
            }

            var startHour = Math.Clamp(StartHour, 0, 23);
            var endHour = Math.Clamp(EndHour, startHour + 1, 24);
            var visibleHours = endHour - startHour;

            var resources = Resources?.ToList() ?? new List<Resource>();
            var events = Events?.ToList() ?? new List<ResourceEvent>();

            // Width is based on hours; height is header band + rows.
            var width = HourWidth * visibleHours;
            var height = Math.Max(TimeHeaderHeight + resources.Count * RowHeight, 1);

            if (!DoubleEquals(_timelineCanvas.Width, width))
            {
                _timelineCanvas.Width = width;
            }

            if (!DoubleEquals(_timelineCanvas.Height, height))
            {
                _timelineCanvas.Height = height;
            }

            _timelineCanvas.Children.Clear();

            DrawGrid(_timelineCanvas, resources.Count, startHour, endHour);
            DrawEvents(_timelineCanvas, resources, events, startHour, endHour);
        }

        private static bool DoubleEquals(double a, double b) => Math.Abs(a - b) < 0.0001;

        private void DrawGrid(Canvas canvas, int rows, int startHour, int endHour)
        {
            // Vertical hour lines + labels.
            for (var h = startHour; h <= endHour; h++)
            {
                var x = (h - startHour) * HourWidth;
                var line = new Line
                {
                    StartPoint = new Point(x, 0),
                    EndPoint = new Point(x, canvas.Height),
                    Stroke = GridLineBrush,
                    StrokeThickness = h % 6 == 0 ? 1.5 : 0.75,
                    Opacity = h % 6 == 0 ? 0.75 : 0.4
                };
                canvas.Children.Add(line);

                if (h < endHour)
                {
                    var text = new TextBlock
                    {
                        Text = $"{h:00}:00",
                        Foreground = GridLineBrush,
                        Margin = new Thickness(0)
                    };
                    text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(text, x + 2);
                    // Center the label vertically inside the header band.
                    var headerY = (TimeHeaderHeight - text.DesiredSize.Height) / 2;
                    Canvas.SetTop(text, headerY);
                    canvas.Children.Add(text);
                }
            }

            // Horizontal row dividers: start AFTER the header band.
            for (var r = 1; r <= rows; r++)
            {
                var y = TimeHeaderHeight + r * RowHeight;
                var hline = new Line
                {
                    StartPoint = new Point(0, y),
                    EndPoint = new Point(canvas.Width, y),
                    Stroke = GridLineBrush,
                    StrokeThickness = 0.75,
                    Opacity = 0.4
                };
                canvas.Children.Add(hline);
            }
        }

        private void DrawEvents(
            Canvas canvas,
            List<Resource> resources,
            List<ResourceEvent> events,
            int startHour,
            int endHour)
        {
            if (resources.Count == 0 || events.Count == 0)
            {
                return;
            }

            var dayStart = new DateTimeOffset(Day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var dayEnd = dayStart.AddDays(1);

            var byResource = events
                .Select(e => ClipToDay(e, dayStart, dayEnd))
                .Where(e => e != null)
                .GroupBy(e => e!.ResourceId)
                .ToDictionary(g => g.Key, g => g!.ToList()!);

            var visibleStart = dayStart.AddHours(startHour);
            var visibleEnd = dayStart.AddHours(endHour);

            for (var r = 0; r < resources.Count; r++)
            {
                var res = resources[r];
                if (!byResource.TryGetValue(res.Id, out var resEvents))
                {
                    continue;
                }

                foreach (var ev in resEvents)
                {
                    var clipped = ClipToWindow(ev!, visibleStart, visibleEnd);
                    if (clipped == null)
                    {
                        continue;
                    }

                    var startSpan = clipped.Start - visibleStart;
                    var endSpan = clipped.End - visibleStart;

                    var x = Math.Max(0, startSpan.TotalHours * HourWidth);
                    var width = Math.Max(2, (endSpan - startSpan).TotalHours * HourWidth);

                    // Row r: vertical origin is header band + r * RowHeight.
                    var rowTop = TimeHeaderHeight + r * RowHeight;
                    var heightRect = Math.Max(4, RowHeight - 8);
                    var y = rowTop + (RowHeight - heightRect) / 2; // center bar within row.

                    var rect = new Rectangle
                    {
                        Fill = clipped.Brush ?? Brushes.SteelBlue,
                        Stroke = Brushes.Transparent,
                        RadiusX = 4,
                        RadiusY = 4,
                        Width = width,
                        Height = heightRect
                    };
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    canvas.Children.Add(rect);

                    if (!string.IsNullOrWhiteSpace(clipped.Title))
                    {
                        var tb = new TextBlock
                        {
                            Text = clipped.Title,
                            Foreground = Brushes.White,
                            Margin = new Thickness(6, 0, 6, 0)
                        };
                        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        var desired = tb.DesiredSize;
                        Canvas.SetLeft(tb, x + 4);
                        Canvas.SetTop(tb, y + (heightRect - desired.Height) / 2);
                        canvas.Children.Add(tb);
                    }
                }
            }
        }

        private static ResourceEvent? ClipToDay(ResourceEvent ev, DateTimeOffset dayStart, DateTimeOffset dayEnd)
        {
            var start = ev.Start < dayStart ? dayStart : ev.Start;
            var end = ev.End > dayEnd ? dayEnd : ev.End;
            if (end <= dayStart || start >= dayEnd)
            {
                return null;
            }

            return new ResourceEvent
            {
                ResourceId = ev.ResourceId,
                Start = start,
                End = end,
                Brush = ev.Brush,
                Title = ev.Title
            };
        }

        private static ResourceEvent? ClipToWindow(ResourceEvent ev, DateTimeOffset windowStart, DateTimeOffset windowEnd)
        {
            var start = ev.Start < windowStart ? windowStart : ev.Start;
            var end = ev.End > windowEnd ? windowEnd : ev.End;
            if (end <= windowStart || start >= windowEnd)
            {
                return null;
            }

            return new ResourceEvent
            {
                ResourceId = ev.ResourceId,
                Start = start,
                End = end,
                Brush = ev.Brush,
                Title = ev.Title
            };
        }
    }
}