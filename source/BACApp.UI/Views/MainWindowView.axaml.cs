using Avalonia.Controls;
using Avalonia.Threading;
using System;

namespace BACApp.UI.Views;

public sealed partial class MainWindowView : Window
{
    public MainWindowView()
    {
        InitializeComponent();

        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        // On some Linux X11 setups without a WM, starting Maximized can produce
        // incorrect client bounds (often visible as a black band). Prefer sizing
        // to the screen working area instead of maximizing.
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var screen = Screens?.ScreenFromWindow(this) ?? Screens?.Primary;
        if (screen is null)
        {
            return;
        }

        // Defer one tick to ensure screen bounds are available and layout has started.
        Dispatcher.UIThread.Post(() =>
        {
            var wa = screen.WorkingArea; // in pixels
            Position = wa.Position;
            Width = wa.Width;
            Height = wa.Height;
        }, DispatcherPriority.Background);
    }
}