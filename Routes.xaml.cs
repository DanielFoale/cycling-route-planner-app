using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Newtonsoft.Json.Linq;
using System.Net;

namespace CyclingRoutePlannerApp;

public partial class RoutesPage : ContentPage
{
    public RoutesPage()
    {
        InitializeComponent();
    }

   
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        try
        {
            map1.IsVisible = false;
            map2.IsVisible = false;
            map3.IsVisible = false;
            RouteName1.IsVisible = false;
            RouteName2.IsVisible = false;
            RouteName3.IsVisible = false;
            map1.MapElements.Clear();
            map2.MapElements.Clear();
            map3.MapElements.Clear();
            string json = new WebClient().DownloadString("https://chirk-rhythm.000webhostapp.com/routeretrieval.php");
            var objects = JArray.Parse(json);
            foreach (JObject item in objects)
            {
                if (item.GetValue("Username").ToString() == User.UserLoggedIn)
                {
                    if (RouteName1.IsVisible == false)
                    {
                        populateMap(RouteName1, map1, item);
                    }
                    else if (RouteName2.IsVisible == false)
                    {
                        populateMap(RouteName2, map2, item);
                    }
                    else if (RouteName3.IsVisible == false)
                    {
                        populateMap(RouteName3, map3, item);
                    }
                }
            }
        }
        catch (WebException)
        {
            DisplayAlert("Network error", "Please check your connection and try again.", "OK");
        }
        
    }

    protected void populateMap(Label RouteName, Microsoft.Maui.Controls.Maps.Map map, JObject item)
    {
        RouteName.Text = item.GetValue("Label").ToString();
        string routeRaw = item.GetValue("AsText(route)").ToString().Remove(0, 11);
        routeRaw = routeRaw.Remove(routeRaw.Length - 1);
        string[] coords = routeRaw.Split(',');
        var mapLine = new Microsoft.Maui.Controls.Maps.Polyline
        {
            StrokeWidth = 8,
            StrokeColor = Color.Parse("#1BA1E2")
        };
        foreach (string coordset in coords)
        {
            string[] longandlat = coordset.Split(' ');

            mapLine.Geopath.Add(new Location(Convert.ToDouble(longandlat[1]), Convert.ToDouble(longandlat[0])));
        }
        var BristolLoc = new Location(51.4545, -2.5879);

        map.MapElements.Add(mapLine);
        map.IsVisible = true;
        RouteName.IsVisible = true;
        MapSpan mapSpan = MapSpan.FromCenterAndRadius(BristolLoc, Distance.FromKilometers(3));
        map.MoveToRegion(mapSpan);
    }
}