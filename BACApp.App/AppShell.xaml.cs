namespace BACApp.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("Login", typeof(Pages.LoginPage));
		Routing.RegisterRoute("MainTabs", typeof(AppShell));
		// Ensure we start at the login page
		_ = GoToAsync("//Login");
	}
}
