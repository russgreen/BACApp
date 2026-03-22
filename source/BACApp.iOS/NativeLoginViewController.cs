using BACApp.Core.DTO;
using BACApp.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace BACApp.iOS;

internal sealed class NativeLoginViewController : UIViewController
{
    private UIImageView _logoImageView = null!;
    private UITextField _usernameTextField = null!;
    private UITextField _passwordTextField = null!;
    private UIButton _loginButton = null!;
    private UILabel _messageLabel = null!;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.BackgroundColor = UIColor.SystemBackground;
        ModalInPresentation = true;
        Title = "Login";

        _logoImageView = new UIImageView
        {
            ContentMode = UIViewContentMode.ScaleAspectFit,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Image = UIImage.FromBundle("LoginLogo")
        };

        _usernameTextField = new UITextField
        {
            Placeholder = "Enter email address",
            BorderStyle = UITextBorderStyle.RoundedRect,
            TextContentType = UITextContentType.Username,
            KeyboardType = UIKeyboardType.EmailAddress,
            AutocorrectionType = UITextAutocorrectionType.No,
            AutocapitalizationType = UITextAutocapitalizationType.None,
            ReturnKeyType = UIReturnKeyType.Next,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        _passwordTextField = new UITextField
        {
            Placeholder = "Enter password",
            BorderStyle = UITextBorderStyle.RoundedRect,
            TextContentType = UITextContentType.Password,
            SecureTextEntry = true,
            AutocorrectionType = UITextAutocorrectionType.No,
            AutocapitalizationType = UITextAutocapitalizationType.None,
            ReturnKeyType = UIReturnKeyType.Go,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        _loginButton = new UIButton(UIButtonType.System);
        _loginButton.SetTitle("Login", UIControlState.Normal);
        _loginButton.TranslatesAutoresizingMaskIntoConstraints = false;

        _messageLabel = new UILabel
        {
            Lines = 0,
            TextColor = UIColor.Red,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        _usernameTextField.ShouldReturn = textField =>
        {
            _passwordTextField.BecomeFirstResponder();
            return true;
        };

        _passwordTextField.ShouldReturn = textField =>
        {
            _ = SubmitAsync();
            return true;
        };

        _loginButton.TouchUpInside += async (_, _) => await SubmitAsync();

        var stack = new UIStackView(new UIView[]
        {
            _logoImageView,
            _usernameTextField,
            _passwordTextField,
            _loginButton,
            _messageLabel
        })
        {
            Axis = UILayoutConstraintAxis.Vertical,
            Spacing = 12,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        View.AddSubview(stack);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            _logoImageView.HeightAnchor.ConstraintEqualTo(96),
            _logoImageView.WidthAnchor.ConstraintEqualTo(96),
            stack.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
            stack.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
            stack.LeadingAnchor.ConstraintGreaterThanOrEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 24),
            stack.TrailingAnchor.ConstraintLessThanOrEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -24),
            stack.WidthAnchor.ConstraintEqualTo(360)
        });
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);
        _usernameTextField.BecomeFirstResponder();
    }

    private async Task SubmitAsync()
    {
        var username = _usernameTextField.Text?.Trim() ?? string.Empty;
        var password = _passwordTextField.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _messageLabel.Text = "Please enter both username/email and password.";
            return;
        }

        try
        {
            SetBusy(true, "Attempting login...");

            var response = await NativeLoginBridge.LoginAsync(username, password);

            if (response is null)
            {
                SetBusy(false, "Login failed. Check credentials.");
                return;
            }

            if (response.Companies is null || response.Companies.Length == 0)
            {
                SetBusy(false, "No valid companies were returned.");
                return;
            }

            if (response.Companies.Length == 1)
            {
                NativeLoginBridge.CompleteLogin(response.Companies[0]);
                SetBusy(false, string.Empty);
                return;
            }

            SetBusy(false, string.Empty);
            PresentCompanyPicker(response.Companies);
        }
        catch (Exception ex)
        {
            SetBusy(false, ex.Message);
        }
    }

    private void PresentCompanyPicker(CompanyDto[] companies)
    {
        var alert = UIAlertController.Create(
            "Select company",
            null,
            UIAlertControllerStyle.ActionSheet);

        foreach (var company in companies.OrderBy(c => c.CompanyName))
        {
            alert.AddAction(UIAlertAction.Create(company.CompanyName, UIAlertActionStyle.Default, _ =>
            {
                NativeLoginBridge.CompleteLogin(company);
            }));
        }

        alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

        var popover = alert.PopoverPresentationController;
        if (popover is not null)
        {
            popover.SourceView = _loginButton;
            popover.SourceRect = _loginButton.Bounds;
        }

        PresentViewController(alert, true, null);
    }

    private void SetBusy(bool isBusy, string message)
    {
        _loginButton.Enabled = !isBusy;
        _usernameTextField.Enabled = !isBusy;
        _passwordTextField.Enabled = !isBusy;
        _messageLabel.Text = message;
    }
}