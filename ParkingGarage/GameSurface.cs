using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using NAudio.Wave;
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
    private readonly Image? _blueCarSprite;
    private readonly Image? _redCarSprite;
    private readonly Image? _greenCarSprite;
    private readonly Image? _yellowCarSprite;
    private readonly Image? _purpleCarSprite;
    private readonly string _idleCarAudioPath;
    private readonly string _movingCarAudioPath;
    private readonly string _reverseBeepAudioPath;
    private readonly string _carCrashAudioPath;
    private IWavePlayer? _idleOutput;
    private AudioFileReader? _idleReader;
    private IWavePlayer? _movingOutput;
    private AudioFileReader? _movingReader;
    private IWavePlayer? _reverseOutput;
    private AudioFileReader? _reverseReader;
    private IWavePlayer? _crashOutput;
    private AudioFileReader? _crashReader;
    private bool _engineLoopWanted;
    private bool _restartingIdle;
    private bool _restartingMoving;
    private bool _restartingReverse;
    private float _idleVolume = 1f;
    private float _movingVolume;

    private const float IdleFadeSeconds = 1.25f;
    private const float MovingFadeInSeconds = 1.25f;
    private const float MovingFadeOutSeconds = 0.75f;

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

        var bluePath = Path.Combine(AppContext.BaseDirectory, "Assets", "BlueCar.png");
        if (File.Exists(bluePath))
            _blueCarSprite = Image.FromFile(bluePath);

        var redPath = Path.Combine(AppContext.BaseDirectory, "Assets", "RedCar.png");
        if (File.Exists(redPath))
            _redCarSprite = Image.FromFile(redPath);

        var greenPath = Path.Combine(AppContext.BaseDirectory, "Assets", "GreenCar.png");
        if (File.Exists(greenPath))
            _greenCarSprite = Image.FromFile(greenPath);

        var yellowPath = Path.Combine(AppContext.BaseDirectory, "Assets", "YellowCar.png");
        if (File.Exists(yellowPath))
            _yellowCarSprite = Image.FromFile(yellowPath);

        var purplePath = Path.Combine(AppContext.BaseDirectory, "Assets", "PurpleCar.png");
        if (File.Exists(purplePath))
            _purpleCarSprite = Image.FromFile(purplePath);
        _idleCarAudioPath = Path.Combine(AppContext.BaseDirectory, "Sound Effects", "Idle Car.mp3");
        _movingCarAudioPath = Path.Combine(AppContext.BaseDirectory, "Sound Effects", "Moving Sound.mp3");
        _reverseBeepAudioPath = Path.Combine(AppContext.BaseDirectory, "Sound Effects", "Reverse Beep.mp3");
        _carCrashAudioPath = Path.Combine(AppContext.BaseDirectory, "Sound Effects", "Car Crash.mp3");

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
        UpdateEngineLoopState(0f);
        Invalidate();
    }

    public void StopSession()
    {
        _timer.Stop();
        _keysDown.Clear();
        _crashOverlay.Visible = false;
        StopEngineLoop();
        StopCrashSound();
    }

    public void NotifyKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Left or Keys.Right or Keys.Up or Keys.Down)
        {
            _keysDown.Add(e.KeyCode);
            UpdateEngineLoopState(0f);
            e.Handled = true;
        }
    }

    public void NotifyKeyUp(KeyEventArgs e)
    {
        _keysDown.Remove(e.KeyCode);
        UpdateEngineLoopState(0f);
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
        UpdateEngineLoopState(0f);
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
        {
            StopEngineLoop();
            return;
        }

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
            StopEngineLoop();
            PlayCrashSoundOnce();
            ShowCrashOverlay();
        }
        else
        {
            UpdateEngineLoopState(dt);
        }

        Invalidate();
    }

    private bool ShouldPlayEngineLoop() =>
        _game is { Phase: GamePhase.Playing, ActiveCar: not null } &&
        !_crashOverlay.Visible &&
        Visible;

    private void UpdateEngineLoopState(float deltaSeconds)
    {
        if (!ShouldPlayEngineLoop())
        {
            StopEngineLoop();
            return;
        }

        EnsureEngineLoopStarted();
        if (_idleReader == null || _movingReader == null)
            return;

        // Default loop state: idle full, moving silent. When Up is held: crossfade to moving.
        var forwardHeld = _keysDown.Contains(Keys.Up);
        var targetIdle = forwardHeld ? 0f : 1f;
        var targetMoving = forwardHeld ? 1f : 0f;
        var idleRate = 1f / IdleFadeSeconds;
        var movingRate = forwardHeld ? 1f / MovingFadeInSeconds : 1f / MovingFadeOutSeconds;

        if (deltaSeconds <= 0f)
            deltaSeconds = 0f;

        _idleVolume = MoveTowards(_idleVolume, targetIdle, idleRate * deltaSeconds);
        _movingVolume = MoveTowards(_movingVolume, targetMoving, movingRate * deltaSeconds);
        _idleReader.Volume = _idleVolume;
        _movingReader.Volume = _movingVolume;

        UpdateReverseBeepState();
    }

    private void EnsureEngineLoopStarted()
    {
        if (_idleOutput != null && _idleReader != null && _movingOutput != null && _movingReader != null)
            return;

        if (!File.Exists(_idleCarAudioPath) || !File.Exists(_movingCarAudioPath))
            return;

        try
        {
            _idleReader = new AudioFileReader(_idleCarAudioPath) { Volume = _idleVolume };
            _movingReader = new AudioFileReader(_movingCarAudioPath) { Volume = _movingVolume };

            _idleOutput = new WaveOutEvent();
            _movingOutput = new WaveOutEvent();
            _idleOutput.PlaybackStopped += IdleOutput_PlaybackStopped;
            _movingOutput.PlaybackStopped += MovingOutput_PlaybackStopped;
            _idleOutput.Init(_idleReader);
            _movingOutput.Init(_movingReader);
            _engineLoopWanted = true;
            _idleOutput.Play();
            _movingOutput.Play();
        }
        catch
        {
            StopEngineLoop();
        }
    }

    private void StopEngineLoop()
    {
        _engineLoopWanted = false;
        _restartingIdle = false;
        _restartingMoving = false;
        _restartingReverse = false;
        _idleVolume = 1f;
        _movingVolume = 0f;

        if (_idleOutput != null)
        {
            _idleOutput.PlaybackStopped -= IdleOutput_PlaybackStopped;
            _idleOutput.Stop();
            _idleOutput.Dispose();
            _idleOutput = null;
        }

        if (_movingOutput != null)
        {
            _movingOutput.PlaybackStopped -= MovingOutput_PlaybackStopped;
            _movingOutput.Stop();
            _movingOutput.Dispose();
            _movingOutput = null;
        }

        _idleReader?.Dispose();
        _idleReader = null;
        _movingReader?.Dispose();
        _movingReader = null;
        StopReverseBeep();
    }

    private void IdleOutput_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (!_engineLoopWanted || _idleOutput == null || _idleReader == null || _restartingIdle)
            return;

        _restartingIdle = true;
        try
        {
            _idleReader.Position = 0;
            _idleOutput.Play();
        }
        catch
        {
            StopEngineLoop();
        }
        finally
        {
            _restartingIdle = false;
        }
    }

    private void MovingOutput_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (!_engineLoopWanted || _movingOutput == null || _movingReader == null || _restartingMoving)
            return;

        _restartingMoving = true;
        try
        {
            _movingReader.Position = 0;
            _movingOutput.Play();
        }
        catch
        {
            StopEngineLoop();
        }
        finally
        {
            _restartingMoving = false;
        }
    }

    private void UpdateReverseBeepState()
    {
        var reverseHeld = _keysDown.Contains(Keys.Down);
        if (!reverseHeld)
        {
            StopReverseBeep();
            return;
        }

        if (_reverseOutput != null && _reverseReader != null)
            return;

        if (!File.Exists(_reverseBeepAudioPath))
            return;

        try
        {
            _reverseReader = new AudioFileReader(_reverseBeepAudioPath);
            _reverseOutput = new WaveOutEvent();
            _reverseOutput.PlaybackStopped += ReverseOutput_PlaybackStopped;
            _reverseOutput.Init(_reverseReader);
            _reverseOutput.Play();
        }
        catch
        {
            StopReverseBeep();
        }
    }

    private void StopReverseBeep()
    {
        _restartingReverse = false;

        if (_reverseOutput != null)
        {
            _reverseOutput.PlaybackStopped -= ReverseOutput_PlaybackStopped;
            _reverseOutput.Stop();
            _reverseOutput.Dispose();
            _reverseOutput = null;
        }

        _reverseReader?.Dispose();
        _reverseReader = null;
    }

    private void ReverseOutput_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (_reverseOutput == null || _reverseReader == null || _restartingReverse || !_keysDown.Contains(Keys.Down))
            return;

        _restartingReverse = true;
        try
        {
            _reverseReader.Position = 0;
            _reverseOutput.Play();
        }
        catch
        {
            StopReverseBeep();
        }
        finally
        {
            _restartingReverse = false;
        }
    }

    private void PlayCrashSoundOnce()
    {
        StopCrashSound();
        if (!File.Exists(_carCrashAudioPath))
            return;

        try
        {
            _crashReader = new AudioFileReader(_carCrashAudioPath) { Volume = 0.8f };
            _crashOutput = new WaveOutEvent();
            _crashOutput.PlaybackStopped += CrashOutput_PlaybackStopped;
            _crashOutput.Init(_crashReader);
            _crashOutput.Play();
        }
        catch
        {
            StopCrashSound();
        }
    }

    private void CrashOutput_PlaybackStopped(object? sender, StoppedEventArgs e) => StopCrashSound();

    private void StopCrashSound()
    {
        if (_crashOutput != null)
        {
            _crashOutput.PlaybackStopped -= CrashOutput_PlaybackStopped;
            _crashOutput.Stop();
            _crashOutput.Dispose();
            _crashOutput = null;
        }

        _crashReader?.Dispose();
        _crashReader = null;
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        if (current < target)
            return MathF.Min(current + maxDelta, target);
        return MathF.Max(current - maxDelta, target);
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

        for (var i = 0; i < _game.Cars.Count; i++)
        {
            var car = _game.Cars[i];
            var sprite = i switch
            {
                0 when _blueCarSprite != null => _blueCarSprite,
                1 when _redCarSprite != null => _redCarSprite,
                2 when _greenCarSprite != null => _greenCarSprite,
                3 when _yellowCarSprite != null => _yellowCarSprite,
                4 when _purpleCarSprite != null => _purpleCarSprite,
                _ => null
            };
            DrawCar(g, linePen, car, sprite);
        }

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

    private static void DrawCar(Graphics g, Pen outlinePen, Car car, Image? sprite)
    {
        var hw = car.VisualWidth * 0.5f;
        var hhVis = car.VisualHeight * 0.5f;
        var state = g.Save();
        try
        {
            g.TranslateTransform(car.CenterX, car.CenterY);
            g.RotateTransform(car.HeadingDegreesClockwiseFromUp);

            if (sprite != null)
            {
                var prevInterp = g.InterpolationMode;
                var prevPixel = g.PixelOffsetMode;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                try
                {
                    var iw = sprite.Width;
                    var ih = sprite.Height;
                    var src = new RectangleF(0f, 0f, iw, ih);
                    var dst = new RectangleF(-hw, -hhVis, car.VisualWidth, car.VisualHeight);
                    g.DrawImage(sprite, dst, src, GraphicsUnit.Pixel);
                }
                finally
                {
                    g.PixelOffsetMode = prevPixel;
                    g.InterpolationMode = prevInterp;
                }
            }
            else
            {
                using var carBrush = new SolidBrush(car.BodyColor);
                g.FillRectangle(carBrush, -hw, -hhVis, car.VisualWidth, car.VisualHeight);
                g.DrawRectangle(outlinePen, -hw, -hhVis, car.VisualWidth, car.VisualHeight);

                var nose = Darken(car.BodyColor, 0.58f);
                using var noseBrush = new SolidBrush(nose);
                var tipY = -hhVis + Math.Max(4f, hhVis * 0.06f);
                var baseY = -hhVis + Math.Max(18f, hhVis * 0.24f);
                var halfW = Math.Max(5f, car.VisualWidth * 0.15f);
                PointF[] nosePoly =
                [
                    new(0, tipY),
                    new(-halfW, baseY),
                    new(halfW, baseY)
                ];
                g.FillPolygon(noseBrush, nosePoly);
            }
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
        {
            StopEngineLoop();
            StopCrashSound();
            _timer.Dispose();
            _blueCarSprite?.Dispose();
            _redCarSprite?.Dispose();
            _greenCarSprite?.Dispose();
            _yellowCarSprite?.Dispose();
            _purpleCarSprite?.Dispose();
        }
        base.Dispose(disposing);
    }
}
