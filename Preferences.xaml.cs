using Microsoft.Maui.Maps;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Net;

namespace CyclingRoutePlannerApp;

public partial class PreferencesPage : ContentPage
{
	public PreferencesPage()
	{
		InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        try
        {
            var RetrievePreferences = new WebClient();
            RetrievePreferences.Headers.Add("User-Agent", ".NET Application CycleThere");
            string json = RetrievePreferences.DownloadString("https://chirk-rhythm.000webhostapp.com/");
            var objects = JArray.Parse(json);
            foreach (JObject item in objects)
            {
                if (item.GetValue("Username").ToString() == User.UserLoggedIn)
                {
                    switch (item.GetValue("Speed").ToString())
                    {
                        case "Fast":
                            User.Speed = "Fast";
                            Fast.IsChecked = true;
                            break;
                        case "Moderate":
                            User.Speed = "Moderate";
                            Moderate.IsChecked = true;
                            break;
                        case "Slow":
                            User.Speed = "Slow";
                            Slow.IsChecked = true;
                            break;
                        default:
                            break;
                    }


                    if (item.GetValue("KeepToCycleTracks").ToString() == "0")
                    {
                        User.KeepToCycleTracks = false;
                        KeepToCycleTracks.IsToggled = false;
                    }
                    else
                    {
                        User.KeepToCycleTracks = true;
                        KeepToCycleTracks.IsToggled = true;
                    }

                    if (item.GetValue("AvoidFastRoads").ToString() == "0")
                    {
                        User.AvoidFastRoads = false;
                        AvoidFastRoads.IsToggled = false;
                    }
                    else
                    {
                        User.AvoidFastRoads = true;
                        AvoidFastRoads.IsToggled = true;
                    }

                    break;
                }
            }
        }
        catch (WebException)
        {
            DisplayAlert("Network error", "Please check your connection and try again.", "OK");
        }
    }

    private static readonly HttpClient client = new HttpClient();

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string Speed;
        string KeepToCycleTracksString;
        string AvoidFastRoadsString;

        if(Fast.IsChecked == true)
        {
            User.Speed = "Fast";
            Speed = "Fast";
        }
        else if(Moderate.IsChecked == true)
        {
            User.Speed = "Fast";
            Speed = "Moderate";
        }
        else
        {
            User.Speed = "Fast";
            Speed = "Slow";
        }

        if (KeepToCycleTracks.IsToggled == true)
        {
            User.KeepToCycleTracks = true;
            KeepToCycleTracksString = "1";
        }
        else
        {
            User.KeepToCycleTracks = false;
            KeepToCycleTracksString = "0";
        }

        if (AvoidFastRoads.IsToggled == true)
        {
            User.AvoidFastRoads = true;
            AvoidFastRoadsString = "1";
        }
        else
        {
            User.AvoidFastRoads = false;
            AvoidFastRoadsString = "0";
        }



        var values = new Dictionary<string, string>
                {
                    { "Username", User.UserLoggedIn },
                    { "Speed",  Speed},
                    { "KeepToCycleTracks", KeepToCycleTracksString },
                    { "AvoidFastRoads", AvoidFastRoadsString }

                };

        var content = new FormUrlEncodedContent(values);

        try
        {
            
            var response = await client.PostAsync("https://chirk-rhythm.000webhostapp.com/update.php", content);
        }
        catch (WebException)
        {
            await DisplayAlert("Network error", "Please check your connection and try again.", "OK");
        }
    }
}