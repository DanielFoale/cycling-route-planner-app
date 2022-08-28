using System.ComponentModel;
using System.Net;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace CyclingRoutePlannerApp;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        error.Text = null;
        if (UsernameEntered() & PasswordEntered())
        {
            try
            {
                var LoginCheck = new WebClient();
                LoginCheck.Headers.Add("User-Agent", ".NET Application CycleThere");
                string json = LoginCheck.DownloadString("https://chirk-rhythm.000webhostapp.com/");
                var objects = JArray.Parse(json);
                bool usernameFound = false;
                foreach (JObject item in objects)
                {
                    if (item.GetValue("Username").ToString() == usernameEntry.Text)
                    {
                        usernameFound = true;
                        if (item.GetValue("Password").ToString() == passwordEntry.Text)
                        {
                            usernameEntry.Text = null;
                            passwordEntry.Text = null;
                            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                        }
                        else
                        {
                            error.Text = "Incorrect password.\n";
                        }
                    }
                }
                if (usernameFound == false)
                    error.Text = "There is no account with that username.\n";
            }
            catch (WebException)
            {
                await DisplayAlert("Network error", "Please check your connection and try again.", "OK");
            }
        }
        
    }
    private async void TapGestureRecongnizer_Tapped(object sender, EventArgs e)
    {
        usernameEntry.Text = null;
        passwordEntry.Text = null;
        await Shell.Current.GoToAsync($"//{nameof(RegistrationPage)}");
    }

    private bool UsernameEntered()
    {
        if (usernameEntry.Text != "" & usernameEntry.Text != null)
            return true;
        else
        {
            error.Text += "Username field empty.\n";
            return false;
        }
    }

    private bool PasswordEntered()
    {
        if (passwordEntry.Text != "" & passwordEntry.Text != null)
            return true;
        else
        {
            if (error.Text != null)
                error.Text += "\n";
            error.Text += "Password field empty.\n";
            return false;
        }
    }
}