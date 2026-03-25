using Avalonia;
using Avalonia.iOS;
using BACApp.Core.Messages;
using BACApp.UI;
using CommunityToolkit.Mvvm.Messaging;
using Foundation;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace BACApp.iOS;

[Register("AppDelegate")]
public sealed class AppDelegate : AvaloniaAppDelegate<App>
{
    private NativeLoginViewController? _nativeLoginController;
    private bool _shouldShowNativeLogin = true;
    private bool _presentationScheduled;
    private readonly NSObject _didBecomeActiveObserver;

    public AppDelegate()
    {
        WeakReferenceMessenger.Default.Register<NativeLoginVisibilityMessage>(this, (_, m) =>
        {
            _shouldShowNativeLogin = m.Value;
            UIApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                if (_shouldShowNativeLogin)
                {
                    ScheduleNativeLoginPresentation();
                }
                else
                {
                    DismissNativeLoginIfNeeded();
                }
            });
        });

        _didBecomeActiveObserver = UIApplication.Notifications.ObserveDidBecomeActive((_, _) =>
        {
            if (_shouldShowNativeLogin)
            {
                ScheduleNativeLoginPresentation();
            }
        });
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    private void ScheduleNativeLoginPresentation()
    {
        if (_presentationScheduled || !_shouldShowNativeLogin || _nativeLoginController is not null)
        {
            return;
        }

        _presentationScheduled = true;

        UIApplication.SharedApplication.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                for (var attempt = 0; attempt < 20; attempt++)
                {
                    if (!_shouldShowNativeLogin || _nativeLoginController is not null)
                    {
                        return;
                    }

                    if (TryPresentNativeLogin())
                    {
                        return;
                    }

                    await Task.Delay(100);
                }
            }
            finally
            {
                _presentationScheduled = false;
            }
        });
    }

    private bool TryPresentNativeLogin()
    {
        var presenter = GetTopViewController();
        if (presenter is null)
        {
            return false;
        }

        if (presenter is NativeLoginViewController nativeLoginViewController)
        {
            _nativeLoginController = nativeLoginViewController;
            return true;
        }

        _nativeLoginController = new NativeLoginViewController
        {
            ModalPresentationStyle = UIModalPresentationStyle.FormSheet
        };

        presenter.PresentViewController(_nativeLoginController, true, null);
        return true;
    }

    private void DismissNativeLoginIfNeeded()
    {
        if (_nativeLoginController is null)
        {
            return;
        }

        var controller = _nativeLoginController;
        _nativeLoginController = null;

        if (controller.PresentingViewController is not null)
        {
            controller.DismissViewController(true, null);
        }
    }

    private static UIViewController? GetTopViewController()
    {
        var window = UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIWindowScene>()
            .Where(scene => scene.ActivationState == UISceneActivationState.ForegroundActive)
            .SelectMany(scene => scene.Windows)
            .FirstOrDefault(window => window.IsKeyWindow);

        var controller = window?.RootViewController;

        while (controller?.PresentedViewController is not null)
        {
            controller = controller.PresentedViewController;
        }

        return controller;
    }
}
