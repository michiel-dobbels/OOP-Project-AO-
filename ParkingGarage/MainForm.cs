namespace ParkingGarage;

public partial class MainForm : Form
{
    private readonly GameSurface _gameSurface;

    public MainForm()
    {
        InitializeComponent();

        _gameSurface = new GameSurface { Dock = DockStyle.Fill, Visible = false };
        Controls.Add(_gameSurface);
        _gameSurface.SendToBack();
        _menuPanel.BringToFront();

        _gameSurface.ExitToMenu += OnGameExitToMenu;

        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        _menuPanel.Visible = false;
        _gameSurface.Visible = true;
        _gameSurface.BringToFront();
        BeginInvoke(() =>
        {
            if (_gameSurface.ClientSize.Width > 0 && _gameSurface.ClientSize.Height > 0)
                _gameSurface.StartSession();
        });
        _gameSurface.Focus();
        Focus();
    }

    private void BtnExit_Click(object? sender, EventArgs e) => Application.Exit();

    private void OnGameExitToMenu()
    {
        _gameSurface.StopSession();
        _gameSurface.Visible = false;
        _menuPanel.Visible = true;
        _menuPanel.BringToFront();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (_gameSurface.Visible)
            _gameSurface.NotifyKeyDown(e);
    }

    private void MainForm_KeyUp(object? sender, KeyEventArgs e)
    {
        if (_gameSurface.Visible)
            _gameSurface.NotifyKeyUp(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_gameSurface.Visible && keyData == Keys.Escape)
        {
            OnGameExitToMenu();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}
