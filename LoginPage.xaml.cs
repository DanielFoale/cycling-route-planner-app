using System.ComponentModel;

namespace CyclingRoutePlannerApp;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}


    private async void Button_Clicked(object sender, EventArgs e)
    {
        usernameEntry.Text = null;
        passwordEntry.Text = null;
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }
    private async void TapGestureRecongnizer_Tapped(object sender, EventArgs e)
    {
        usernameEntry.Text = null;
        passwordEntry.Text = null;
        await Shell.Current.GoToAsync($"//{nameof(RegistrationPage)}");
    }
}