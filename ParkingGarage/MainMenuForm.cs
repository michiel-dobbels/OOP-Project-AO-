namespace ParkingGarage;

public partial class MainMenuForm : Form
{
    public MainMenuForm()
    {
        InitializeComponent();
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        using var game = new GameForm();
        Hide();
        game.ShowDialog(this);
        Show();
    }

    private void BtnExit_Click(object? sender, EventArgs e) => Close();
}
