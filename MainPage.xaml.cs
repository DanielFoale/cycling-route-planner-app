namespace CyclingRoutePlannerApp;

public partial class MainPage : ContentPage
{
    public MainPage()
	{
		InitializeComponent();
	}

    private void OnBtnClicked(object sender, EventArgs e)
    {
        GoBtn.Text = $"generating...";

        SemanticScreenReader.Announce(GoBtn.Text);
    }
}

