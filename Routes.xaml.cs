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
        string json = new WebClient().DownloadString("https://chirk-rhythm.000webhostapp.com/routeretrieval.php");
        var objects = JArray.Parse(json);
        foreach (JObject item in objects)
        {
            if (item.GetValue("Username").ToString() == User.UserLoggedIn)
            {
                if(RouteName1.IsVisible == false)
                {
                    RouteName1.Text = item.GetValue("Label").ToString();
                    string k = item.GetValue("AsText(route)").ToString().Remove(0, 11);
                    k = k.Remove(k.Length - 1);
                    string[] coords = k.Split(',');
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

                    MapSpan mapSpan = MapSpan.FromCenterAndRadius(BristolLoc, Distance.FromKilometers(3));
                    map1.MoveToRegion(mapSpan);
                    map1.MapElements.Add(mapLine);
                    map1.IsVisible = true;
                    RouteName1.IsVisible = true;
                }
                else if(RouteName2.IsVisible == false)
                {
                    RouteName2.Text = item.GetValue("Label").ToString();
                    string k = item.GetValue("AsText(route)").ToString().Remove(0, 11);
                    k = k.Remove(k.Length - 1);
                    string[] coords = k.Split(',');
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

                    MapSpan mapSpan = MapSpan.FromCenterAndRadius(BristolLoc, Distance.FromKilometers(3));
                    map2.MoveToRegion(mapSpan);

                    map2.MapElements.Add(mapLine);
                    map2.IsVisible = true;
                    RouteName2.IsVisible = true;
                }
                else if (RouteName2.IsVisible == false)
                {
                    RouteName3.Text = item.GetValue("Label").ToString();
                    string k = item.GetValue("AsText(route)").ToString().Remove(0, 11);
                    k = k.Remove(k.Length - 1);
                    string[] coords = k.Split(',');
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

                    MapSpan mapSpan = MapSpan.FromCenterAndRadius(BristolLoc, Distance.FromKilometers(3));
                    map3.MoveToRegion(mapSpan);
                    map3.MapElements.Add(mapLine);
                    map3.IsVisible = true;
                    RouteName3.IsVisible = true;
                }
            }
        }
    }
}