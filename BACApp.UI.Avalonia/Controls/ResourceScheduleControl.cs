using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using BACApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace BACApp.UI.Avalonia.Controls
{
    public class ResourceScheduleControl : TemplatedControl
    {
        public static readonly StyledProperty<IEnumerable<Resource>?> ResourcesProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, IEnumerable<Resource>?>(nameof(Resources));

        public static readonly StyledProperty<IEnumerable<Event>?> EventsProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, IEnumerable<Event>?>(nameof(Events));

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

        public static readonly StyledProperty<double> MinHourWidthProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, double>(nameof(MinHourWidth), 40);

        public static readonly StyledProperty<ICommand?> ResourceClickCommandProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, ICommand?>(nameof(ResourceClickCommand));

        public static readonly StyledProperty<ICommand?> EventClickCommandProperty =
            AvaloniaProperty.Register<ResourceScheduleControl, ICommand?>(nameof(EventClickCommand));

        public IEnumerable<Resource>? Resources
        {
            get => GetValue(ResourcesProperty);
            set => SetValue(ResourcesProperty, value);
        }

        public IEnumerable<Event>? Events
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

        public double MinHourWidth
        {
            get => GetValue(MinHourWidthProperty);
            set => SetValue(MinHourWidthProperty, value);
        }

        public ICommand? ResourceClickCommand
        {
            get => GetValue(ResourceClickCommandProperty);
            set => SetValue(ResourceClickCommandProperty, value);
        }

        public ICommand? EventClickCommand
        {
            get => GetValue(EventClickCommandProperty);
            set => SetValue(EventClickCommandProperty, value);
        }

        private ScrollViewer? _scrollViewer;
        private ScrollViewer? _resourceScrollViewer;
        private ItemsControl? _resourceList;
        private Canvas? _timelineCanvas;
        private Rect _lastBounds;

        private bool _isSyncingVerticalScroll;

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
            MinHourWidthProperty.Changed.AddClassHandler<ResourceScheduleControl>((c, _) => c.Redraw());

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
            _resourceScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ResourceScrollViewer");
            _resourceList = e.NameScope.Find<ItemsControl>("PART_ResourceList");
            _timelineCanvas = e.NameScope.Find<Canvas>("PART_TimelineCanvas");

            if (_resourceList != null)
            {
                _resourceList.ItemsSource = Resources ?? Enumerable.Empty<Resource>();
            }

            HookScrollSync();

            Redraw();
        }

        private void HookScrollSync()
        {
            if (_scrollViewer == null || _resourceScrollViewer == null)
            {
                return;
            }

            _scrollViewer.PropertyChanged -= OnTimelineScrollViewerPropertyChanged;
            _resourceScrollViewer.PropertyChanged -= OnResourceScrollViewerPropertyChanged;

            _scrollViewer.PropertyChanged += OnTimelineScrollViewerPropertyChanged;
            _resourceScrollViewer.PropertyChanged += OnResourceScrollViewerPropertyChanged;

            SyncVerticalOffset(_scrollViewer, _resourceScrollViewer);
        }

        private void OnTimelineScrollViewerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != ScrollViewer.OffsetProperty || _scrollViewer == null || _resourceScrollViewer == null)
            {
                return;
            }

            SyncVerticalOffset(_scrollViewer, _resourceScrollViewer);
        }

        private void OnResourceScrollViewerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != ScrollViewer.OffsetProperty || _scrollViewer == null || _resourceScrollViewer == null)
            {
                return;
            }

            SyncVerticalOffset(_resourceScrollViewer, _scrollViewer);
        }

        private void SyncVerticalOffset(ScrollViewer from, ScrollViewer to)
        {
            if (_isSyncingVerticalScroll)
            {
                return;
            }

            try
            {
                _isSyncingVerticalScroll = true;

                var fromOffset = from.Offset;
                var toOffset = to.Offset;

                if (!DoubleEquals(fromOffset.Y, toOffset.Y))
                {
                    to.Offset = new Vector(toOffset.X, fromOffset.Y);
                }
            }
            finally
            {
                _isSyncingVerticalScroll = false;
            }
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
            var events = Events?.ToList() ?? new List<Event>();

            // Calculate available width from ScrollViewer
            var availableWidth = _scrollViewer?.Viewport.Width ?? Bounds.Width;
            if (availableWidth <= 0)
            {
                availableWidth = HourWidth * visibleHours; // fallback
            }

            // Compute actual hour width: expand to fill, but respect minimum
            var computedHourWidth = Math.Max(MinHourWidth, availableWidth / visibleHours);

            var width = computedHourWidth * visibleHours;
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

            DrawGrid(_timelineCanvas, resources.Count, startHour, endHour, computedHourWidth);
            DrawEvents(_timelineCanvas, resources, events, startHour, endHour, computedHourWidth);
        }

        private static bool DoubleEquals(double a, double b) => Math.Abs(a - b) < 0.0001;

        private void DrawGrid(Canvas canvas, int rows, int startHour, int endHour, double hourWidth)
        {
            var totalMinutes = (endHour - startHour) * 60;

            // Draw vertical lines for every 15 minutes
            for (int minute = 0; minute <= totalMinutes; minute += 15)
            {
                var hours = minute / 60.0;
                var x = hours * hourWidth;
                var isHourMark = minute % 60 == 0;

                var line = new Line
                {
                    // Start below the header to avoid obscuring hour labels
                    StartPoint = new Point(x, TimeHeaderHeight),
                    EndPoint = new Point(x, canvas.Height),
                    Stroke = GridLineBrush,
                    StrokeThickness = isHourMark ? 1.5 : 0.5,
                    Opacity = isHourMark ? 0.75 : 0.3
                };
                canvas.Children.Add(line);
            }

            // Draw hour labels (only for hour marks)
            for (var h = startHour; h < endHour; h++)
            {
                var x = (h - startHour) * hourWidth;
                var text = new TextBlock
                {
                    Text = $"{h:00}:00",
                    Foreground = GridLineBrush,
                    Margin = new Thickness(0)
                };
                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(text, x + 2);
                var headerY = (TimeHeaderHeight - text.DesiredSize.Height) / 2;
                Canvas.SetTop(text, headerY);
                canvas.Children.Add(text);
            }

            // Horizontal row dividers: start AFTER the header band
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
            List<Event> events,
            int startHour,
            int endHour,
            double hourWidth)
        {
            if (resources.Count == 0 || events.Count == 0)
            {
                return;
            }

            var dayStart = new DateTimeOffset(Day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var dayEnd = dayStart.AddDays(1);

            var byResource = events
                .Select(e => new { Original = e, Clipped = ClipToDay(e, dayStart, dayEnd) })
                .Where(x => x.Clipped != null)
                .GroupBy(x => x.Clipped!.ResourceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var visibleStart = dayStart.AddHours(startHour);
            var visibleEnd = dayStart.AddHours(endHour);

            for (var r = 0; r < resources.Count; r++)
            {
                var resource = resources[r];
                if (!byResource.TryGetValue(resource.Id, out var resEvents))
                {
                    continue;
                }

                // Draw background events first (behind), then normal events.
                foreach (var item in resEvents
                    .OrderBy(x => string.Equals(x.Original.Rendering, "background", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenBy(x => x.Original.StartTime))
                {
                    var clipped = ClipToWindow(item.Clipped!, visibleStart, visibleEnd);
                    if (clipped == null)
                    {
                        continue;
                    }

                    var startSpan = clipped.StartTime - visibleStart;
                    var endSpan = clipped.EndTime - visibleStart;

                    var x = Math.Max(0, startSpan.TotalHours * hourWidth);
                    var width = Math.Max(2, (endSpan - startSpan).TotalHours * hourWidth);

                    var rowTop = TimeHeaderHeight + r * RowHeight;
                    var isBackground = string.Equals(item.Original.Rendering, "background", StringComparison.OrdinalIgnoreCase);
                    var heightRect = isBackground ? Math.Max(1, RowHeight - 2) : Math.Max(4, RowHeight - 8);
                    var y = isBackground ? rowTop + 1 : rowTop + (RowHeight - heightRect) / 2;

                    // Create a border container for the event
                    var eventBorder = new Border
                    {
                        Background = item.Original.BackgroundBrush ?? Brushes.WhiteSmoke,
                        BorderBrush = item.Original.BorderBrush ?? Brushes.Black,
                        BorderThickness = isBackground ? new Thickness(0) : new Thickness(1),
                        CornerRadius = isBackground ? new CornerRadius(0) : new CornerRadius(4),
                        Width = width,
                        Height = heightRect,
                        Cursor = new Cursor(StandardCursorType.Hand),
                        Opacity = isBackground ? 0.35 : 1.0,
                        IsHitTestVisible = !isBackground
                    };

                    // Store reference to the ORIGINAL event, not the clipped one
                    var originalEvent = item.Original;

                    if (!isBackground)
                    {
                        eventBorder.PointerPressed += (_, e) =>
                        {
                            if (EventClickCommand?.CanExecute(originalEvent) == true)
                            {
                                EventClickCommand.Execute(originalEvent);
                                e.Handled = true;
                            }
                        };
                    }

                    Canvas.SetLeft(eventBorder, x);
                    Canvas.SetTop(eventBorder, y);

                    if (!isBackground && !string.IsNullOrWhiteSpace(clipped.Title))
                    {
                        var tb = new TextBlock
                        {
                            Text = clipped.Title,
                            Foreground = item.Original.TextBrush,
                            Margin = new Thickness(6, 0, 6, 0),
                            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
                            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left
                        };
                        eventBorder.Child = tb;
                    }

                    canvas.Children.Add(eventBorder);
                }
            }
        }

        private static Event? ClipToDay(Event ev, DateTimeOffset dayStart, DateTimeOffset dayEnd)
        {
            var start = ev.StartTime < dayStart ? dayStart : ev.StartTime;
            var end = ev.EndTime > dayEnd ? dayEnd : ev.EndTime;
            if (end <= dayStart || start >= dayEnd)
            {
                return null;
            }

            return new Event
            {
                ResourceId = ev.ResourceId,
                Start = start.ToString(),
                End = end.ToString(),
                //BackgroundColor = ev.BackgroundBrush,
                Title = ev.Title
            };
        }

        private static Event? ClipToWindow(Event ev, DateTimeOffset windowStart, DateTimeOffset windowEnd)
        {
            var start = ev.StartTime < windowStart ? windowStart : ev.StartTime;
            var end = ev.EndTime > windowEnd ? windowEnd : ev.EndTime;
            if (end <= windowStart || start >= windowEnd)
            {
                return null;
            }

            return new Event
            {
                ResourceId = ev.ResourceId,
                Start = start.ToString(),
                End = end.ToString(),
                //BackgroundBrush = ev.BackgroundBrush,
                Title = ev.Title
            };
        }
    }
}