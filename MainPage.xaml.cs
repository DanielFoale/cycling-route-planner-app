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

namespace CyclingRoutePlannerApp;



public partial class MainPage : ContentPage
{
    

    public MainPage()
    {
        InitializeComponent();
        stuff();
    }

    async void stuff()
    {
        RoadNetwork bristol = new RoadNetwork();

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

            Crossroad bit = new Crossroad(Convert.ToInt64(array1[0]), Convert.ToDouble(array1[1]), Convert.ToDouble(array1[2]));

            bristol.crossroadList.Add(bit);
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

            Road bit = new Road(Convert.ToInt64(array1[1]), Convert.ToInt64(array1[2]), Convert.ToDouble(array1[3]), Convert.ToInt64(array1[0]), Convert.ToInt32(array1[7]), Convert.ToInt32(array1[8]));

            bristol.roadList.Add(bit);
        }

        streamReader2.Close();
    }

    class Crossroad
    {
        public Crossroad(long id, double latitude, double longitude)
        {
            this.id = id;
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public long id;
        public double latitude;
        public double longitude;
    }

    class Road
    {
        public Road(long start, long end, double length, long roadPartOf, int bikeInfoForward, int bikeInfoBackward)
        {
            this.start = start;
            this.end = end;
            this.length = length;
            this.roadPartOf = roadPartOf;
        }

        public long start;
        public long end;
        public long roadPartOf;
        public double length;
        bool isOneWay;
        bool isCyclePath;
    }

    class RoadNetwork
    {
        public List<Crossroad> crossroadList = new List<Crossroad>();
        public List<Road> roadList = new List<Road>();
        public Crossroad currentCrossroad;
    }

    private void OnBtnClicked(object sender, EventArgs e)
    {
        if(startIndex != -1 & endIndex != -1)
        {
            GoBtn.Text = "Generating...";
            double startLat = coordStart[0, startIndex];
            double startLon = coordStart[1, startIndex];
            double endLat = coordEnd[0, endIndex];
            double endLon = coordEnd[1, endIndex];

            SemanticScreenReader.Announce(GoBtn.Text);
        }
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

