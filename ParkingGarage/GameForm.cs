using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace ParkingGarage;

public class GameForm : Form
{
    private readonly ParkingGame _game;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 };
    private readonly HashSet<Keys> _keysDown = [];
    private readonly Stopwatch _sw = new();
    private long _lastTicks;

    private static readonly Color GreenLed = Color.FromArgb(80, 220, 120);
    private static readonly Color RedLed = Color.FromArgb(240, 70, 70);

    public GameForm()
    {
        _game = new ParkingGame(960, 600);
        Text = "Parking — simulatie";
        ClientSize = new Size(960, 600);
        BackColor = AppColors.FieldBackground;
        ForeColor = Color.WhiteSmoke;
        DoubleBuffered = true;
        KeyPreview = true;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        _timer.Tick += OnTick;
        Load += (_, _) =>
        {
            _sw.Start();
            _lastTicks = _sw.ElapsedTicks;
            _timer.Start();
        };
        FormClosed += (_, _) => _timer.Stop();

        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Left or Keys.Right or Keys.Up or Keys.Down)
        {
            _keysDown.Add(e.KeyCode);
            e.Handled = true;
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        _keysDown.Remove(e.KeyCode);
    }

    private void OnTick(object? sender, EventArgs e)
    {
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

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var counterFont = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        var free = _game.AvailableSpotCount;
        g.DrawString($"beschikbare plaatsen: {free}", counterFont, Brushes.WhiteSmoke, 16f, 12f);

        using var linePen = new Pen(Color.White, 2f) { LineJoin = LineJoin.Miter };
        foreach (var spot in _game.Spots)
        {
            var r = spot.SpotBounds;
            g.DrawRectangle(linePen, r.X, r.Y, r.Width, r.Height);

            var occupied = spot.IsOccupiedBy(_game.Car.AxisAlignedBounds);
            var fill = occupied ? RedLed : GreenLed;
            using var brush = new SolidBrush(fill);
            const float ledR = 9f;
            g.FillEllipse(brush, spot.LedCenter.X - ledR, spot.LedCenter.Y - ledR, ledR * 2, ledR * 2);
            g.DrawEllipse(linePen, spot.LedCenter.X - ledR, spot.LedCenter.Y - ledR, ledR * 2, ledR * 2);
        }

        var car = _game.Car;
        var hw = car.Width * 0.5f;
        var hh = car.Height * 0.5f;
        var state = g.Save();
        try
        {
            g.TranslateTransform(car.CenterX, car.CenterY);
            g.RotateTransform(car.HeadingDegreesClockwiseFromUp);

            using var carBrush = new SolidBrush(Color.FromArgb(200, 200, 210));
            g.FillRectangle(carBrush, -hw, -hh, car.Width, car.Height);
            g.DrawRectangle(linePen, -hw, -hh, car.Width, car.Height);

            using var noseBrush = new SolidBrush(Color.FromArgb(130, 130, 145));
            PointF[] nose =
            [
                new(0, -hh + 3),
                new(-5, -hh + 16),
                new(5, -hh + 16)
            ];
            g.FillPolygon(noseBrush, nose);
        }
        finally
        {
            g.Restore(state);
        }

        using var hintFont = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
        using var hintBrush = new SolidBrush(Color.FromArgb(160, 160, 160));
        g.DrawString("← / → = sturen   |   ↑ / ↓ = vooruit / achteruit   |   ESC = sluiten", hintFont, hintBrush, 16f, ClientSize.Height - 36f);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (ClientSize.Width > 0 && ClientSize.Height > 0)
            _game.Resize(ClientSize.Width, ClientSize.Height);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            Close();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}
