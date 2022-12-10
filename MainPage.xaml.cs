using Newtonsoft.Json.Linq;
using QuikGraph;
using System.Net;
using System.Globalization;
using CsvHelper;
using Microsoft.Maui.Maps;

namespace CyclingRoutePlannerApp;

public partial class MainPage : ContentPage
{


    public MainPage()
    {
        InitializeComponent();
        ParseMapData();
        
    }
    public class Road
    {
        public Road(double length, long roadPartOf, int bikeStatus, string speed, string linestring, bool forward)
        {
            this.length = length;
            this.roadPartOf = roadPartOf;
            this.linestring = linestring;
            this.forward = forward;
            if (bikeStatus == 5)
            {
                isCycleTrack = true;
            }
            else
            {
                isCycleTrack = false;
            }

            if(speed == "30 mph" || speed == "40 mph" || speed == "50 mph" || speed == "60 mph" || speed == "70 mph")
            {
                fastSpeed = true;
            }
            else
            {
                fastSpeed = false;
            }
        }
        public bool forward;
        public long roadPartOf;
        public double length;
        public bool isCycleTrack;
        public bool fastSpeed;
        public string linestring;
    }

    public class FinalPathNodes
    {
        public FinalPathNodes(double lat, double lon)
        {
            latitude = lat;
            longitude = lon;
        }

        public double latitude;
        public double longitude;
    }

    public class Junction
    {
        public Junction(long id, double longitude, double lat)
        {
            this.id = id;
            this.lat = lat;
            this.longitude = longitude;
        }

        public long id;
        public double lat;
        public double longitude;
        public double f = -1;
        public double g = -1;
        public double h = -1;
        public Junction parent;
        public Road road;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        startPoint.Text = "";
        startAddressesList.ItemsSource = new List<string>();
        endAddressesList.ItemsSource = new List<string>();
        endPoint.Text = "";
        DistanceDisplay.Text = "";
        TimeDisplay.Text = "";
        map.MapElements.Clear();
        var BristolLoc = new Location(51.4545, -2.5879);

        MapSpan mapSpan = MapSpan.FromCenterAndRadius(BristolLoc, Distance.FromKilometers(3));
        map.MoveToRegion(mapSpan);

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
                            break;
                        case "Moderate":
                            User.Speed = "Moderate";
                            break;
                        case "Slow":
                            User.Speed = "Slow";
                            break;
                        default:
                            break;
                    }


                    if (item.GetValue("KeepToCycleTracks").ToString() == "0")
                    {
                        User.KeepToCycleTracks = false;
                    }
                    else
                    {
                        User.KeepToCycleTracks = true;
                    }

                    if (item.GetValue("AvoidFastRoads").ToString() == "0")
                    {
                        User.AvoidFastRoads = false;
                    }
                    else
                    {
                        User.AvoidFastRoads = true;
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
    

    public AdjacencyGraph<Junction, TaggedEdge<Junction, Road>>[] graphs = new AdjacencyGraph<Junction, TaggedEdge<Junction, Road>>[1];
    public IDictionary<long, Junction> Junctions = new Dictionary<long, Junction>();

    async void ParseMapData()
    {


    var graph = new AdjacencyGraph<Junction, TaggedEdge<Junction, Road>>();

        Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync("nodes.csv");
        using var streamReader = new StreamReader(fileStream);
        using var csvReader = new CsvReader(streamReader, CultureInfo.CurrentCulture);

        string value;

        csvReader.Read();

        while (csvReader.Read())
        {
            string[] array1 = new string[3];

            for (int i = 0; csvReader.TryGetField<string>(i, out value); i++)
            {
                array1[i] = value;
            }

            Junction junction = new Junction(Convert.ToInt64(array1[0]), Convert.ToDouble(array1[1]), Convert.ToDouble(array1[2]));

            Junctions.Add(Convert.ToInt64(array1[0]), junction);

            graph.AddVertex(junction);
        }

        streamReader.Close();

        Stream fileStream2 = await FileSystem.Current.OpenAppPackageFileAsync("edges.csv");
        using var streamReader2 = new StreamReader(fileStream2);
        using var csvReader2 = new CsvReader(streamReader2, CultureInfo.CurrentCulture);

        string value2;

        csvReader2.Read();

        while (csvReader2.Read())
        {
            string[] array1 = new string[11];

            for (int i = 0; csvReader2.TryGetField<string>(i, out value2); i++)
            {
                array1[i] = value2;
            }

           
                if (Convert.ToInt32(array1[7]) != 0)
            {
                Road road = new Road(Convert.ToDouble(array1[3]), Convert.ToInt64(array1[0]), Convert.ToInt32(array1[7]), array1[10], array1[9], true);
                var edge = new TaggedEdge<Junction, Road>(Junctions[Convert.ToInt64(array1[1])], Junctions[Convert.ToInt64(array1[2])], road);
                graph.AddEdge(edge);
            }
            if (Convert.ToInt32(array1[8]) != 0)
            {
                Road road = new Road(Convert.ToDouble(array1[3]), Convert.ToInt64(array1[0]), Convert.ToInt32(array1[8]), array1[10], array1[9], false);

                var edge = new TaggedEdge<Junction, Road>(Junctions[Convert.ToInt64(array1[2])], Junctions[Convert.ToInt64(array1[1])], road);
                graph.AddEdge(edge);
            }


        }

        streamReader2.Close();
        graphs[0] = graph;
    }


    private void OnNewRouteBtnClicked(object sender, EventArgs e)
    {
        if (startIndex != -1 & endIndex != -1)
        {
            
            double startLat = coordStart[0, startIndex];
            double startLon = coordStart[1, startIndex];
            double endLat = coordEnd[0, endIndex];
            double endLon = coordEnd[1, endIndex];


            map.MapElements.Clear();

            PathFinder(nearest_node(startLat, startLon), nearest_node(endLat, endLon));
        }
    }

    public static string routeAsString;

    private static readonly HttpClient client = new HttpClient();

    private async void OnSaveRouteBtnClicked(object sender, EventArgs e)
    {
        if (routeAsString!=null)
        {
            try
            {
                string label = await DisplayPromptAsync("Route Name", "Enter here");

                var values = new Dictionary<string, string>
                {
                    { "Username", User.UserLoggedIn },
                    { "Label", label },
                    { "Linestring", routeAsString }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("https://chirk-rhythm.000webhostapp.com/routeentry.php", content);
                var responseString = await response.Content.ReadAsStringAsync();
            }
            catch (WebException)
            {
                await DisplayAlert("Network error", "Please check your connection and try again.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Generate a route first.", "OK");
        }
    }

        public void PathFinder(Junction start_node, Junction goal_node)
    {
        List<Junction> openList = new List<Junction>();
        List<Junction> closedList = new List<Junction>();
        List<FinalPathNodes> parts = new List<FinalPathNodes>();

        openList.Add(start_node);

        openList[0].g = 0;

        openList[0].h = haversine(start_node.lat, start_node.longitude, goal_node.lat, goal_node.longitude); 
            


        openList[0].f = openList[0].h;

        Junction currentNode = openList[0];

        while (openList != null)
        {
            double lowestF = -1;
            foreach (Junction junction in openList)
            {
                if (lowestF == -1 || junction.f < lowestF)
                {
                    lowestF = junction.f;
                    currentNode = junction;
                }
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);
    
            if(currentNode == goal_node)
            {
                double totalDistance = 0;
                do
                {
                    
                    Junction previousNode = currentNode.parent;

                    Road f = currentNode.road;
                    totalDistance += f.length;

                    string k = f.linestring.Remove(0, 11);
                    k = k.Remove(k.Length - 1);
                    k = k.Replace(", ", ",");
                    string[] coords = k.Split(',');
                    if (f.forward)
                    {
                        for (int i = (coords.Length-1); i > -1; i--)
                        {
                            string[] final = coords[i].Split(' ');
                            FinalPathNodes m = new FinalPathNodes(Convert.ToDouble(final[1]), Convert.ToDouble(final[0]));
                            parts.Add(m);
                        }
                    }
                    else
                    {

                        foreach (string coord in coords)
                        {
                            string[] final = coord.Split(' ');
                            FinalPathNodes m = new FinalPathNodes(Convert.ToDouble(final[1]), Convert.ToDouble(final[0]));
                            parts.Add(m);
                        }
                    }

                    currentNode = previousNode;

                } while (currentNode.parent != null);

                var mapLine = new Microsoft.Maui.Controls.Maps.Polyline
                {
                    StrokeWidth = 8,
                    StrokeColor = Color.Parse("#1BA1E2")
                };

                routeAsString = "";

                foreach (FinalPathNodes item in parts)
                {
                    routeAsString += (item.longitude.ToString()+" "+item.latitude.ToString()+",");
                    mapLine.Geopath.Add(new Location(item.latitude, item.longitude));
                }
                routeAsString = routeAsString.Remove(routeAsString.Length-1);

                map.MapElements.Add(mapLine);

                var BristolLoc = new Location(((start_node.lat + goal_node.lat) / 2), ((start_node.longitude + goal_node.longitude) / 2));

                double maxdistance = haversine(start_node.lat, start_node.longitude, goal_node.lat, goal_node.longitude);

                MapSpan mapSpan = MapSpan.FromCenterAndRadius(BristolLoc, Distance.FromKilometers(maxdistance/2000+0.25));
                map.MoveToRegion(mapSpan);

                if (totalDistance < 1000)
                {
                    DistanceDisplay.Text = Convert.ToString(Math.Round(totalDistance)) + " metres     ";
                }
                else
                {
                    DistanceDisplay.Text = Convert.ToString(Math.Round((totalDistance/1000),1)) + " kilometres     ";
                }

                try
                {
                    double speed = 15;
                    var RetrievePreferences = new WebClient();
                    RetrievePreferences.Headers.Add("User-Agent", ".NET Application CycleThere");
                    string json = RetrievePreferences.DownloadString("https://chirk-rhythm.000webhostapp.com/speed.php");
                    var objects = JArray.Parse(json);
                    foreach (JObject item in objects)
                    {
                        if (item.GetValue("SpeedDescription").ToString() == User.Speed)
                        {
                            speed = Convert.ToDouble(item.GetValue("SpeedNumber"));
                            break;
                        }
                    }

                    TimeDisplay.Text = Math.Round((totalDistance / 1000 / speed * 60), 0) + " minutes";

                }
                catch (WebException)
                {
                    DisplayAlert("Network error", "Please check your connection and try again.", "OK");
                }

                break;
            }
            else
            {
                foreach (var edge in graphs[0].OutEdges(currentNode))
                {
                    double cycletrackmultiplier = 1;
                    double roadspeedmultiplier = 1;
                    if (User.KeepToCycleTracks == true & edge.Tag.isCycleTrack == false)
                    {
                        cycletrackmultiplier = 3;
                    }
                    if (User.AvoidFastRoads == true & edge.Tag.fastSpeed == true)
                    {
                        roadspeedmultiplier = 10;
                    }
                    double provisionalH = haversine(edge.Target.lat, edge.Target.longitude, goal_node.lat, goal_node.longitude);
                    double provisionalG = currentNode.g + Math.Max(cycletrackmultiplier,roadspeedmultiplier) * (edge.Tag.length);
                    double provisionalF = provisionalH + provisionalG;
                    if (openList.Contains(edge.Target))
                    {
                        if(provisionalF < edge.Target.f)
                        {
                            edge.Target.g = provisionalG;
                            edge.Target.f = provisionalF;
                            edge.Target.parent = currentNode;
                            edge.Target.road = edge.Tag;
                        }
                    }
                    else if (closedList.Contains(edge.Target))
                    {
                        if (provisionalF < edge.Target.f)
                        {
                            closedList.Remove(edge.Target);
                            edge.Target.g = provisionalG;
                            edge.Target.f = provisionalF;
                            edge.Target.parent = currentNode;
                            edge.Target.road = edge.Tag;
                            openList.Add(edge.Target);
                        }
                    }
                    else
                    {
                        edge.Target.g = provisionalG;
                        edge.Target.f = provisionalF;
                        edge.Target.parent = currentNode;
                        edge.Target.road = edge.Tag;
                        openList.Add(edge.Target);
                    }
                }
            }

            
        }
        // failed

    }

    public Junction nearest_node(double lat, double lon)
    {
        double distanceToNode = -1;
        Junction nearestNode = Junctions[291583393];
        foreach(var node in graphs[0].Vertices)
        {

            if (distanceToNode == -1 || haversine(lat, lon, node.lat, node.longitude) < distanceToNode)
            {
                distanceToNode = haversine(lat, lon, node.lat, node.longitude);
                nearestNode = node;
            }
        }

        return nearestNode;
    }

    public double haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371e3; // metres
        double φ1 = lat1 * Math.PI / 180; // φ, λ in radians
        double φ2 = lat2 * Math.PI / 180;
        double Δφ = (lat2 - lat1) * Math.PI / 180.0;
        double Δλ = (lon2 - lon1) * Math.PI / 180.0;

        double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                  Math.Cos(φ1) * Math.Cos(φ2) *
                  Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double d = R * c; // in metres

        return d;
    }

    public double[,] coordStart = new double[2, 20];

    public double[,] coordEnd = new double[2, 20];

    private void OnEnterStart(object sender, EventArgs e)
    {
        try
        {
            string startLocation = startPoint.Text;

            string page = $"https://nominatim.openstreetmap.org/search?q={startLocation}&format=json";

            var getaddresses = new WebClient();
            getaddresses.Headers.Add("User-Agent", ".NET Application CycleThere");
            string result = getaddresses.DownloadString(page);

            var objects = JArray.Parse(result);

            List<string> pickerSource = new List<string>();

            int count = -1;

            foreach (JObject item in objects)
            {
                count++;
                pickerSource.Add(item.GetValue("display_name").ToString());
                coordStart[0,count] = Convert.ToDouble(item.GetValue("lat"));
                coordStart[1,count] = Convert.ToDouble(item.GetValue("lon"));
            }


            if (count == -1)
            {
                DisplayAlert("Search error", "No matching address found.\nPlease Try Again.", "OK");
            }
            else if (count == 0)
            {
                startAddressesList.ItemsSource = pickerSource;
                startAddressesList.SelectedIndex = 0;
            }
            else
            {
                startAddressesList.ItemsSource = pickerSource;
                startAddressesList.Focus();
            }
        }
        catch (WebException)
        {
            DisplayAlert("Network error", "Please check your connection and try again.", "OK");
        }

    }

    public int startIndex = -1;


    void OnStartAddressPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        startPoint.Focus();
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        startIndex = selectedIndex;
    }

    public int endIndex = -1;

    void OnEndAddressPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        endPoint.Focus();
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        endIndex = selectedIndex;
    }

    private void OnEnterEnd(object sender, EventArgs e)
    {
        try
        {
            string endLocation = endPoint.Text;

            string page = $"https://nominatim.openstreetmap.org/search?q={endLocation}&format=json";

            var getaddresses = new WebClient();
            getaddresses.Headers.Add("User-Agent", ".NET Application CycleThere");
            string result = getaddresses.DownloadString(page);

            var objects = JArray.Parse(result);


            List<string> pickerSource = new List<string>();

            int count = -1;

            foreach (JObject item in objects)
            {
                count++;
                pickerSource.Add(item.GetValue("display_name").ToString());
                coordEnd[0, count] = Convert.ToDouble(item.GetValue("lat"));
                coordEnd[1, count] = Convert.ToDouble(item.GetValue("lon"));
            }

            if (count == -1)
            {
                DisplayAlert("Search error", "No matching address found.\nPlease Try Again.", "OK");
                endPoint.Focus();
            }
            else if (count == 0)
            {
                endAddressesList.ItemsSource = pickerSource;
                endAddressesList.SelectedIndex = 0;
            }
            else
            {
                endAddressesList.ItemsSource = pickerSource;
                endAddressesList.Focus();
            }
        }
        catch (WebException)
        {
            DisplayAlert("Network error", "Please check your connection and try again.", "OK");
        }
    }
    

}

