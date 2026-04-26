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
        _gameSurface.ExitProgram += OnGameExitProgram;

        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
        CenterMenuPanel();
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

    private void OnGameExitProgram() => Application.Exit();

    private void CenterMenuPanel()
    {
        _menuPanel.PerformLayout();
        var menuWidth = _menuPanel.PreferredSize.Width;
        var menuHeight = _menuPanel.PreferredSize.Height;
        _menuPanel.SetBounds(
            Math.Max(0, (ClientSize.Width - menuWidth) / 2),
            Math.Max(0, (ClientSize.Height - menuHeight) / 2),
            menuWidth,
            menuHeight);
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
            _gameSurface.TogglePauseMenu();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        CenterMenuPanel();
    }
}
