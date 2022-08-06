namespace CyclingRoutePlannerApp;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage()
	{
		InitializeComponent();
	}

    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
}