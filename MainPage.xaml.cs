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

    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
}

