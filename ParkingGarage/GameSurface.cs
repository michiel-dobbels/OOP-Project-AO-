using System.Diagnostics;
using System.Drawing.Drawing2D;
using ParkingGarage.Models;

namespace ParkingGarage;

/// <summary>Parking simulation view (same pixel-sized spots/cars; field size comes from panel).</summary>
public class GameSurface : Panel
{
    private ParkingGame? _game;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 };
    private readonly HashSet<Keys> _keysDown = [];
    private readonly Stopwatch _sw = new();
    private long _lastTicks;

    private readonly Panel _crashOverlay;
    private readonly TableLayoutPanel _crashContent;
    private readonly Label _crashLabel;
    private readonly Button _btnHerstart;
    private readonly Button _btnStopSimulatie;

    private static readonly Color GreenLed = Color.FromArgb(80, 220, 120);
    private static readonly Color RedLed = Color.FromArgb(240, 70, 70);

    public event Action? ExitToMenu;

    public GameSurface()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();
        BackColor = AppColors.FieldBackground;
        ForeColor = Color.WhiteSmoke;
        TabStop = true;

        _crashLabel = new Label
        {
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
            Text = "",
            Padding = new Padding(24, 16, 24, 8),
            BackColor = Color.Transparent,
            MaximumSize = new Size(720, 0)
        };

        _btnHerstart = new Button
        {
            Text = "Herstart",
            AutoSize = true,
            BackColor = AppColors.PanelControl,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(8),
            Padding = new Padding(24, 10, 24, 10),
            UseVisualStyleBackColor = false
        };
        _btnHerstart.FlatAppearance.BorderColor = Color.Gainsboro;
        _btnHerstart.Click += (_, _) => OnHerstart();

        _btnStopSimulatie = new Button
        {
            Text = "stop simulatie",
            AutoSize = true,
            BackColor = AppColors.PanelControl,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(8),
            Padding = new Padding(24, 10, 24, 10),
            UseVisualStyleBackColor = false
        };
        _btnStopSimulatie.FlatAppearance.BorderColor = Color.Gainsboro;
        _btnStopSimulatie.Click += (_, _) => ExitToMenu?.Invoke();

        var buttonRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = Color.Transparent
        };
        buttonRow.Controls.Add(_btnHerstart);
        buttonRow.Controls.Add(_btnStopSimulatie);

        _crashContent = new TableLayoutPanel
        {
            Dock = DockStyle.None,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        _crashContent.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _crashContent.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _crashContent.Controls.Add(_crashLabel, 0, 0);
        _crashContent.Controls.Add(buttonRow, 0, 1);

        _crashOverlay = new Panel
        {
            Dock = DockStyle.Fill,
            Visible = false,
            BackColor = Color.FromArgb(235, 22, 28, 36)
        };
        _crashOverlay.Controls.Add(_crashContent);
        _crashOverlay.Resize += (_, _) => CenterCrashContent();

        Controls.Add(_crashOverlay);
        _crashOverlay.BringToFront();

        _timer.Tick += OnTick;
    }

    public void StartSession()
    {
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;

        _game = new ParkingGame(ClientSize.Width, ClientSize.Height);
        _crashOverlay.Visible = false;
        _sw.Restart();
        _lastTicks = _sw.ElapsedTicks;
        _timer.Start();
        Invalidate();
    }

    public void StopSession()
    {
        _timer.Stop();
        _keysDown.Clear();
        _crashOverlay.Visible = false;
    }

    public void NotifyKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Left or Keys.Right or Keys.Up or Keys.Down)
        {
            _keysDown.Add(e.KeyCode);
            e.Handled = true;
        }
    }

    public void NotifyKeyUp(KeyEventArgs e)
    {
        _keysDown.Remove(e.KeyCode);
        if (e.KeyCode == Keys.Space && _game?.Phase == GamePhase.Playing)
        {
            _game.TryParkCurrentCar();
            e.Handled = true;
        }
    }

    private void CenterCrashContent()
    {
        _crashContent.PerformLayout();
        var w = _crashContent.PreferredSize.Width > 0 ? _crashContent.PreferredSize.Width : _crashContent.Width;
        var h = _crashContent.PreferredSize.Height > 0 ? _crashContent.PreferredSize.Height : _crashContent.Height;
        _crashContent.SetBounds(
            Math.Max(0, (_crashOverlay.ClientSize.Width - w) / 2),
            Math.Max(0, (_crashOverlay.ClientSize.Height - h) / 2),
            w,
            h);
    }

    private void OnHerstart()
    {
        _crashOverlay.Visible = false;
        _game?.ResetSimulation();
        _lastTicks = _sw.ElapsedTicks;
        _timer.Start();
        Invalidate();
    }

    private void ShowCrashOverlay()
    {
        _crashLabel.Text = _game?.LastCrashKind == CrashKind.Wall
            ? "Auto is tegen de muur gereden, hopelijk heeft niemand het gezien."
            : "Auto's gebotst, haal het aanrijdingsformulier uit het handschoenkastje.";
        _crashOverlay.Visible = true;
        _crashOverlay.BringToFront();
        CenterCrashContent();
        Invalidate();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_game == null || _game.Phase == GamePhase.Crashed)
            return;

        var now = _sw.ElapsedTicks;
        var dt = (now - _lastTicks) / (float)Stopwatch.Frequency;
        _lastTicks = now;
        if (dt > 0.1f)
            dt = 0.1f;

        _game.Update(
            dt,
            _keysDown.Contains(Keys.Left),
            _keysDown.Contains(Keys.Right),
            _keysDown.Contains(Keys.Up),
            _keysDown.Contains(Keys.Down));

        if (_game.Phase == GamePhase.Crashed)
        {
            _timer.Stop();
            ShowCrashOverlay();
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_game == null)
            return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var counterFont = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        var free = _game.AvailableSpotCount;
        g.DrawString($"beschikbare plaatsen: {free}", counterFont, Brushes.WhiteSmoke, 16f, 12f);

        using var linePen = new Pen(Color.White, 2f) { LineJoin = LineJoin.Miter };
        DrawParkingLanes(g, linePen, _game.Spots);

        foreach (var spot in _game.Spots)
        {
            var occupied = false;
            foreach (var car in _game.Cars)
            {
                if (spot.IsOccupiedBy(car.AxisAlignedBounds))
                {
                    occupied = true;
                    break;
                }
            }

            var fill = occupied ? RedLed : GreenLed;
            using var brush = new SolidBrush(fill);
            const float ledR = 9f;
            g.FillEllipse(brush, spot.LedCenter.X - ledR, spot.LedCenter.Y - ledR, ledR * 2, ledR * 2);
            g.DrawEllipse(linePen, spot.LedCenter.X - ledR, spot.LedCenter.Y - ledR, ledR * 2, ledR * 2);
        }

        foreach (var car in _game.Cars)
            DrawCar(g, linePen, car);

        using var hintFont = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
        using var hintBrush = new SolidBrush(Color.FromArgb(160, 160, 160));
        g.DrawString(
            "↑ vooruit / ↓ achteruit — ← / → sturen tijdens rijden (omgekeerd bij achteruit) — Spatie parkeren — ESC = menu",
            hintFont,
            hintBrush,
            16f,
            ClientSize.Height - 36f);
    }

    private static void DrawParkingLanes(Graphics g, Pen pen, ParkingSpot[] spots)
    {
        if (spots.Length == 0)
            return;

        var r0 = spots[0].SpotBounds;
        var w = r0.Width;
        var h = r0.Height;
        var left = r0.Left;
        var top = r0.Top;
        var totalW = spots.Length * w;

        g.DrawLine(pen, left, top, left + totalW, top);
        for (var i = 0; i <= spots.Length; i++)
        {
            var x = left + i * w;
            g.DrawLine(pen, x, top, x, top + h);
        }
    }

    private static void DrawCar(Graphics g, Pen outlinePen, Car car)
    {
        var hw = car.Width * 0.5f;
        var hh = car.Height * 0.5f;
        var state = g.Save();
        try
        {
            g.TranslateTransform(car.CenterX, car.CenterY);
            g.RotateTransform(car.HeadingDegreesClockwiseFromUp);

            using var carBrush = new SolidBrush(car.BodyColor);
            g.FillRectangle(carBrush, -hw, -hh, car.Width, car.Height);
            g.DrawRectangle(outlinePen, -hw, -hh, car.Width, car.Height);

            var nose = Darken(car.BodyColor, 0.58f);
            using var noseBrush = new SolidBrush(nose);
            var tipY = -hh + Math.Max(4f, hh * 0.06f);
            var baseY = -hh + Math.Max(18f, hh * 0.24f);
            var halfW = Math.Max(5f, car.Width * 0.15f);
            PointF[] nosePoly =
            [
                new(0, tipY),
                new(-halfW, baseY),
                new(halfW, baseY)
            ];
            g.FillPolygon(noseBrush, nosePoly);
        }
        finally
        {
            g.Restore(state);
        }
    }

    private static Color Darken(Color c, float factor) =>
        Color.FromArgb(
            c.A,
            (int)Math.Clamp(c.R * factor, 0, 255),
            (int)Math.Clamp(c.G * factor, 0, 255),
            (int)Math.Clamp(c.B * factor, 0, 255));

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_game != null && ClientSize.Width > 0 && ClientSize.Height > 0)
            _game.Resize(ClientSize.Width, ClientSize.Height);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _timer.Dispose();
        base.Dispose(disposing);
    }
}
