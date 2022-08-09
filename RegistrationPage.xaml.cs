namespace CyclingRoutePlannerApp;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage()
	{
		InitializeComponent();
	}

    private async void Label_Clicked(object sender, EventArgs e)
    {
        passwordEntry.Text = null;
        usernameEntry.Text = null;
        passwordComplexity.Text = null;
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
    private async void Button_Clicked(object sender, EventArgs e)
    {
        passwordComplexity.Text = null;
        if (UsernameValid() & ComplexEnough())
        {
            passwordEntry.Text = null;
            usernameEntry.Text = null;
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
    }
    private bool ComplexEnough()
    {
        bool containUpper = false;
        bool containLower = false;
        bool correctLength = false;
        bool containNumber = false;
        if (passwordEntry.Text != null & passwordEntry.Text != "")
        {
            foreach (char character in passwordEntry.Text)
            {
                if (character > 64 & character < 91)
                    containUpper = true;
                if (character > 96 & character < 123)
                    containLower = true;
                if (character > 47 & character < 58)
                    containNumber = true;
            }
            if (passwordEntry.Text.Length > 7 & passwordEntry.Text.Length < 21)
                correctLength = true;
        }
        else
        {
            if (passwordComplexity.Text != null)
                passwordComplexity.Text += "\n";
            passwordComplexity.Text += "Password field empty.\n";
            return false;
        }
        if (!containUpper)
        {
            if (passwordComplexity.Text != null)
                passwordComplexity.Text += "\n";
            passwordComplexity.Text += "Password requires an uppercase character.";
        }
        if (!containLower)
        {
            if (passwordComplexity.Text != null)
                passwordComplexity.Text += "\n";
            passwordComplexity.Text += "Passsword requires a lowercase character.";
        }
        if (!correctLength)
        {
            if (passwordComplexity.Text != null)
                passwordComplexity.Text += "\n";
            passwordComplexity.Text += "Password must be between 8 and 20 characters long.";
        }
        if (!containNumber)
        {
            if (passwordComplexity.Text != null)
                passwordComplexity.Text += "\n";
            passwordComplexity.Text += "Password requires a numeric character.";
        }

        if (passwordComplexity.Text != null)
            passwordComplexity.Text += " \n";

        return (containUpper & containLower & correctLength & containNumber);
    }
    private bool UsernameValid()
    {
        if (usernameEntry.Text == null || usernameEntry.Text == "")
        {
            if (passwordComplexity.Text != null)
                passwordComplexity.Text += "\n";
            passwordComplexity.Text += "Username field empty.\n";
            return false;
        }
        if (usernameEntry.Text.Length > 2 & usernameEntry.Text.Length < 16)
            return true;
        else
        {
            if (passwordComplexity.Text != null)
                passwordComplexity.Text += "\n";
            passwordComplexity.Text += "Username must be between 3 and 15 characters.\n";
            return false;
        }
    }
}