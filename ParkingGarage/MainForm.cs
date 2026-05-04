using System.Drawing;

namespace ParkingGarage;

public partial class MainForm : Form
{
    private readonly GameSurface _gameSurface;
    private readonly System.Windows.Forms.Timer _titlePulseTimer = new() { Interval = 380 };
    private static readonly Color[] TitlePulseColors =
    [
        Color.FromArgb(255, 214, 120),
        Color.FromArgb(120, 210, 255),
        Color.FromArgb(160, 255, 190),
        Color.FromArgb(255, 170, 150),
    ];

    private int _titlePulseIndex;

    public MainForm()
    {
        InitializeComponent();

        _gameSurface = new GameSurface { Dock = DockStyle.Fill, Visible = false };
        Controls.Add(_gameSurface);
        _gameSurface.SendToBack();
        _menuPanel.BringToFront();
        _creditLabel.BringToFront();

        _gameSurface.ExitToMenu += OnGameExitToMenu;
        _gameSurface.ExitProgram += OnGameExitProgram;

        _titlePulseTimer.Tick += TitlePulseTimer_Tick;
        _titlePulseTimer.Start();

        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
        CenterMenuPanel();
        PositionCreditLabel();
        FormClosed += (_, _) =>
        {
            _titlePulseTimer.Stop();
            _titlePulseTimer.Dispose();
        };
    }

    private void TitlePulseTimer_Tick(object? sender, EventArgs e)
    {
        if (!_menuPanel.Visible)
            return;
        _titlePulseIndex = (_titlePulseIndex + 1) % TitlePulseColors.Length;
        _lblTitle.ForeColor = TitlePulseColors[_titlePulseIndex];
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        _titlePulseTimer.Stop();
        _menuPanel.Visible = false;
        _gameSurface.Visible = true;
        _gameSurface.BringToFront();
        _creditLabel.BringToFront();
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
        _creditLabel.BringToFront();
        _titlePulseTimer.Start();
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

    private void PositionCreditLabel()
    {
        const int pad = 10;
        _creditLabel.PerformLayout();
        _creditLabel.Location = new Point(
            Math.Max(pad, ClientSize.Width - _creditLabel.Width - pad),
            Math.Max(pad, ClientSize.Height - _creditLabel.Height - pad));
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
        PositionCreditLabel();
    }
}
