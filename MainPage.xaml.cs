using Microsoft.Maui;
using Newtonsoft.Json.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph.Collections;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Globalization;
using CsvHelper;
using System.Formats.Asn1;
using Google.Protobuf.WellKnownTypes;
using System.IO;
using NetTopologySuite.Triangulate;

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
        public Road(double length, long roadPartOf, int bikeStatus)
        {
            this.length = length;
            this.roadPartOf = roadPartOf;
            if (bikeStatus != 2)
            {
                isCyclePath = true;
            }
            else
            {
                isCyclePath = false;
            }
        }
        public long roadPartOf;
        public double length;
        public bool isCyclePath;
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
            string[] array1 = new string[10];

            for (int i = 0; csvReader2.TryGetField<string>(i, out value2); i++)
            {
                array1[i] = value2;
            }

           
                if (Convert.ToInt32(array1[7]) != 0)
            {
                Road road = new Road(Convert.ToDouble(array1[3]), Convert.ToInt64(array1[0]), Convert.ToInt32(array1[7]));
                var edge = new TaggedEdge<Junction, Road>(Junctions[Convert.ToInt64(array1[1])], Junctions[Convert.ToInt64(array1[2])], road);
                graph.AddEdge(edge);
            }
            if (Convert.ToInt32(array1[8]) != 0)
            {
                Road road = new Road(Convert.ToDouble(array1[3]), Convert.ToInt64(array1[0]), Convert.ToInt32(array1[8]));

                var edge = new TaggedEdge<Junction, Road>(Junctions[Convert.ToInt64(array1[2])], Junctions[Convert.ToInt64(array1[1])], road);
                graph.AddEdge(edge);
            }


        }

        streamReader2.Close();
        graphs[0] = graph;
    }


    private void OnBtnClicked(object sender, EventArgs e)
    {
        if (startIndex != -1 & endIndex != -1)
        {
            GoBtn.Text = "Generating...";
            double startLat = coordStart[0, startIndex];
            double startLon = coordStart[1, startIndex];
            double endLat = coordEnd[0, endIndex];
            double endLon = coordEnd[1, endIndex];

            SemanticScreenReader.Announce(GoBtn.Text);

            PathFinder();
        }
    }

    public void PathFinder()
    {
        List<Junction> openList = new List<Junction>();
        List<Junction> closedList = new List<Junction>();

        Junction goal_node = Junctions[7371478630];
        
        Junction start_node = Junctions[291583393];

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
                string k = "";
                string p = "";
                do
                {
                    k += currentNode.id;
                    k += "\n";
                    p += ($"[{currentNode.longitude},{currentNode.lat}],");
                    currentNode = currentNode.parent;

                } while (currentNode != start_node);
                k += start_node.id;
                p += ($"[{start_node.longitude},{start_node.lat}]");
                break;
            }
            else
            {
                foreach (var edge in graphs[0].OutEdges(currentNode))
                {
                    
                    double provisionalH = haversine(edge.Target.lat, edge.Target.longitude, goal_node.lat, goal_node.longitude);
                    double provisionalG = currentNode.g + edge.Tag.length;
                    double provisionalF = provisionalH + provisionalG;
                    if (openList.Contains(edge.Target))
                    {
                        if(provisionalF < edge.Target.f)
                        {
                            edge.Target.g = provisionalG;
                            edge.Target.f = provisionalF;
                            edge.Target.parent = currentNode;
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
                            openList.Add(edge.Target);
                        }
                    }
                    else
                    {
                        edge.Target.g = provisionalG;
                        edge.Target.f = provisionalF;
                        edge.Target.parent = currentNode;
                        openList.Add(edge.Target);
                    }
                }
            }

            
        }
        // failed

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

    private async void OnEnterStart(object sender, EventArgs e)
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

    private async void OnEnterEnd(object sender, EventArgs e)
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

