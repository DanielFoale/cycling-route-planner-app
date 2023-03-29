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
        public Road(double length, int bikeStatus, string speed, string linestring, bool forward)
        {
            this.length = length;
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
        private bool forward;
        private bool isCycleTrack;
        private bool fastSpeed;
        private double length;
        private string linestring;

        public bool getIsForward()
        {
            return forward;
        }

        public bool getIsCycleTrack()
        {
            return isCycleTrack;
        }
        public bool getIsFastSpeed()
        {
            return fastSpeed;
        }
        public double getLength()
        {
            return length;
        }

        public string getLinestring()
        {
            return linestring;
        }
    }

    public class Node
    {
        public Node(double lat, double lon)
        {
            this.lat = lat;
            this.lon = lon;
        }

        private double lat;
        private double lon;

        public double getLat()
        {
            return lat;
        }

        public double getLon()
        {
            return lon;
        }
    }

    public class Junction : Node
    {
        public Junction(double lon, double lat)
            :base(lat, lon)
        {
            
            g = 0;
            f = -1;
        }
        private double g;
        private double f;
        private Junction parent;
        private Road road;

        public double getF()
        {
            return f;
        }

        public double getG()
        {
            return g;
        }
        public Road getRoad()
        {
            return road;
        }

        public Junction getParent()
        {
            return parent;
        }

        public void setF(double f)
        {
            this.f = f;
        }

        public void switchPath(double g, double f, Junction parent, Road road)
        {
            this.g = g;
            this.f = f;
            this.parent = parent;
            this.road = road;

        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        routeAsString = null;
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
                    User.Speed = item.GetValue("Speed").ToString();

                    User.SpeedNumber = Convert.ToDouble(item.GetValue("SpeedNumber").ToString());

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
    

    private AdjacencyGraph<Junction, TaggedEdge<Junction, Road>>[] graphs = new AdjacencyGraph<Junction, TaggedEdge<Junction, Road>>[1];
    private IDictionary<long, Junction> Junctions = new Dictionary<long, Junction>();

    async protected void ParseMapData()
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

            Junction junction = new Junction(Convert.ToDouble(array1[1]), Convert.ToDouble(array1[2]));

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
                Road road = new Road(Convert.ToDouble(array1[3]), Convert.ToInt32(array1[7]), array1[10], array1[9], true);
                var edge = new TaggedEdge<Junction, Road>(Junctions[Convert.ToInt64(array1[1])], Junctions[Convert.ToInt64(array1[2])], road);
                graph.AddEdge(edge);
            }
            if (Convert.ToInt32(array1[8]) != 0)
            {
                Road road = new Road(Convert.ToDouble(array1[3]), Convert.ToInt32(array1[8]), array1[10], array1[9], false);

                var edge = new TaggedEdge<Junction, Road>(Junctions[Convert.ToInt64(array1[2])], Junctions[Convert.ToInt64(array1[1])], road);
                graph.AddEdge(edge);
            }


        }

        streamReader2.Close();
        graphs[0] = graph;
    }


    private async void OnNewRouteBtnClicked(object sender, EventArgs e)
    {
        if (startIndex != -1 & endIndex != -1)
        {
            
            double startLat = coordStart[0, startIndex];
            double startLon = coordStart[1, startIndex];
            double endLat = coordEnd[0, endIndex];
            double endLon = coordEnd[1, endIndex];


            map.MapElements.Clear();

            Junction start_node = Nearest_Node(startLat, startLon);

            Junction end_node = Nearest_Node(endLat, endLon);

            if(start_node != end_node)
            {
                PathFinder(Nearest_Node(startLat, startLon), Nearest_Node(endLat, endLon));
            }
            else
            {
                await DisplayAlert("Error", "Start and end point are too close together or one or more is not within Bristol.", "OK");
            }

        }
        else
        {
            await DisplayAlert("Error", "Pick start and end point first.", "OK");
        }
    }

    private static string routeAsString;

    private static readonly HttpClient httpClient = new HttpClient();

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

                var response = await httpClient.PostAsync("https://chirk-rhythm.000webhostapp.com/routeentry.php", content);
                var responseString = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Success", "Route saved.", "OK");
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

    protected void PathFinder(Junction start_node, Junction goal_node)
    {
        bool pathFound = false;

        List<Junction> openList = new List<Junction>();
        List<Junction> closedList = new List<Junction>();

        openList.Add(start_node);

        double H = haversine(start_node.getLat(), start_node.getLon(), goal_node.getLat(), goal_node.getLon()); 
            
        openList[0].setF(H + openList[0].getG());

        Junction currentNode = openList[0];

        var watch = System.Diagnostics.Stopwatch.StartNew();

        while (openList != null && watch.ElapsedMilliseconds<20000)
        {
            double lowestF = -1;
            foreach (Junction junction in openList)
            {
                if (lowestF == -1 || junction.getF() < lowestF)
                {
                    lowestF = junction.getF();
                    currentNode = junction;
                }
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);
    
            if(currentNode == goal_node)
            {
                pathFound = true;
                DisplayRoute(currentNode, start_node, goal_node);
                break; 
            }
            else
            {
                foreach (var edge in graphs[0].OutEdges(currentNode))
                {
                    double notacycletrackmultiplier = 1;
                    double roadspeedmultiplier = 1;
                    if (User.KeepToCycleTracks == true & edge.Tag.getIsCycleTrack() == false)
                    {
                        notacycletrackmultiplier = 3;
                    }
                    if (User.AvoidFastRoads == true & edge.Tag.getIsFastSpeed() == true)
                    {
                        roadspeedmultiplier = 10;
                    }
                    H = haversine(edge.Target.getLat(), edge.Target.getLon(), goal_node.getLat(), goal_node.getLon());
                    double provisionalG = currentNode.getG() + Math.Max(notacycletrackmultiplier,roadspeedmultiplier) * (edge.Tag.getLength());
                    double provisionalF = H + provisionalG;
                    if (openList.Contains(edge.Target))
                    {
                        if(provisionalF < edge.Target.getF())
                        {
                            edge.Target.switchPath(provisionalG, provisionalF, currentNode, edge.Tag);
                        }
                    }
                    else if (closedList.Contains(edge.Target))
                    {
                        if (provisionalF < edge.Target.getF())
                        {
                            closedList.Remove(edge.Target);
                            edge.Target.switchPath(provisionalG, provisionalF, currentNode, edge.Tag);
                            openList.Add(edge.Target);
                        }
                    }
                    else
                    {
                        edge.Target.switchPath(provisionalG, provisionalF, currentNode, edge.Tag);
                        openList.Add(edge.Target);
                    }
                }
            }
        }
        if (!pathFound)
        {
            DisplayAlert("Route generation error", "Route likely not fully accessible by bike.", "OK");
        }
    }

    private void DisplayRoute(Junction currentNode, Junction start_node, Junction goal_node)
    {
        List<Node> finalNodesList = new List<Node>();

        double totalDistance = 0;
        do
        {

            Junction previousNode = currentNode.getParent();

            Road previousRoad = currentNode.getRoad();
            totalDistance += previousRoad.getLength();

            string previousRoadLinestring = previousRoad.getLinestring().Remove(0, 11);
            previousRoadLinestring = previousRoadLinestring.Remove(previousRoadLinestring.Length - 1);
            previousRoadLinestring = previousRoadLinestring.Replace(", ", ",");
            string[] coords = previousRoadLinestring.Split(',');
            if (previousRoad.getIsForward())
            {
                for (int i = (coords.Length - 1); i > -1; i--)
                {
                    string[] finalCoordSet = coords[i].Split(' ');
                    Node nodeForPath = new Node(Convert.ToDouble(finalCoordSet[1]), Convert.ToDouble(finalCoordSet[0]));
                    finalNodesList.Add(nodeForPath);
                }
            }
            else
            {

                foreach (string coord in coords)
                {
                    string[] finalCoordSet = coord.Split(' ');
                    Node nodeForPath = new Node(Convert.ToDouble(finalCoordSet[1]), Convert.ToDouble(finalCoordSet[0]));
                    finalNodesList.Add(nodeForPath);
                }
            }

            currentNode = previousNode;

        } while (currentNode.getParent() != null);

        var mapLine = new Microsoft.Maui.Controls.Maps.Polyline
        {
            StrokeWidth = 8,
            StrokeColor = Color.Parse("#1BA1E2")
        };

        routeAsString = "";

        foreach (Node item in finalNodesList)
        {
            routeAsString += (item.getLon().ToString() + " " + item.getLat().ToString() + ",");
            mapLine.Geopath.Add(new Location(item.getLat(), item.getLon()));
        }
        routeAsString = routeAsString.Remove(routeAsString.Length - 1);

        map.MapElements.Add(mapLine);

        var BristolLoc = new Location(((start_node.getLat() + goal_node.getLat()) / 2), ((start_node.getLon()+ goal_node.getLon()) / 2));

        double maxdistance = haversine(start_node.getLat(), start_node.getLon(), goal_node.getLat(), goal_node.getLon());

        MapSpan mapSpan = MapSpan.FromCenterAndRadius(BristolLoc, Distance.FromKilometers(maxdistance / 2000 + 0.25));
        map.MoveToRegion(mapSpan);

        if (totalDistance < 1000)
        {
            DistanceDisplay.Text = Convert.ToString(Math.Round(totalDistance)) + " metres     ";
        }
        else
        {
            DistanceDisplay.Text = Convert.ToString(Math.Round((totalDistance / 1000), 1)) + " kilometres     ";
        }

            
            TimeDisplay.Text = Math.Round((totalDistance / 1000 / User.SpeedNumber * 60), 0) + " minutes";

    }

protected Junction Nearest_Node(double lat, double lon)
    {
        double distanceToNode = -1;
        Junction nearestNode = Junctions[291583393];
        foreach(var node in graphs[0].Vertices)
        {

            if (distanceToNode == -1 || haversine(lat, lon, node.getLat(), node.getLon()) < distanceToNode)
            {
                distanceToNode = haversine(lat, lon, node.getLat(), node.getLon());
                nearestNode = node;
            }
        }

        return nearestNode;
    }

    protected double haversine(double lat1, double lon1, double lat2, double lon2)
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

    private double[,] coordStart = new double[2, 20];

    private double[,] coordEnd = new double[2, 20];

    private int startIndex = -1;

    private int endIndex = -1;
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

    private void OnStartAddressPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        startPoint.Focus();
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        startIndex = selectedIndex;
    }


    private void OnEndAddressPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        endPoint.Focus();
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        endIndex = selectedIndex;
    }

    

}

