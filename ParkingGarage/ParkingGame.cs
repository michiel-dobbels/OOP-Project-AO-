using System.Drawing;
using ParkingGarage.Models;

namespace ParkingGarage;

public enum GamePhase
{
    Playing,
    Crashed
}

public enum CrashKind
{
    None,
    CarCollision,
    Wall
}

public class ParkingGame
{
    public const int MaxCars = 5;

    private const float SpotWidth = 151.2f;
    private const float SpotHeight = 230.4f;
    /// Collision + sprite; prior chain, then +30% (×1.3).
    private const float CarWidth = (SpotWidth - 4f) * 0.75f * 0.8f * 0.9f * 0.95f * 1.25f * 2.6f * 0.4f * 1.5f * 0.9f * 0.95f * 0.95f * 0.9f * 0.75f * 1.3f;
    private const float CarLength = (SpotHeight - 6f) * 0.85f * 0.8f * 0.9f * 0.95f * 1.25f * 2.6f * 0.4f * 1.5f * 0.9f * 0.95f * 0.95f * 0.9f * 0.75f * 1.3f;
    /// <summary>Draw length (sprites); collision length is 5% shorter so contact matches artwork.</summary>
    private const float CarLengthPhysics = CarLength * 1.1f;
    private const float CarLengthCollision = CarLengthPhysics * 0.95f;
    private const float Margin = 40f;
    private const float AisleBelowSpots = 158.4f;

    private static readonly Color[] CarFlashColors =
    [
        Color.FromArgb(66, 165, 245),
        Color.FromArgb(239, 83, 80),
        Color.FromArgb(102, 187, 106),
        Color.FromArgb(255, 238, 88),
        Color.FromArgb(171, 71, 188)
    ];

    private readonly List<Car> _cars = [];
    private float _spawnCenterY;

    public ParkingGame(int clientWidth, int clientHeight)
    {
        ClientWidth = clientWidth;
        ClientHeight = clientHeight;
        LayoutSpotsAndPlayArea();
        ResetSimulation();
    }

    public int ClientWidth { get; private set; }
    public int ClientHeight { get; private set; }

    public GamePhase Phase { get; private set; } = GamePhase.Playing;

    public CrashKind LastCrashKind { get; private set; }

    public Car? ActiveCar { get; private set; }

    public IReadOnlyList<Car> Cars => _cars;

    public ParkingSpot[] Spots { get; private set; } = [];

    public RectangleF PlayArea { get; private set; }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;
        if (width == ClientWidth && height == ClientHeight)
            return;

        ClientWidth = width;
        ClientHeight = height;
        LayoutSpotsAndPlayArea();
        ResetSimulation();
    }

    public void ResetSimulation()
    {
        Phase = GamePhase.Playing;
        LastCrashKind = CrashKind.None;
        _cars.Clear();
        var first = CreateCarAtSpawn(colorIndex: 0);
        _cars.Add(first);
        ActiveCar = first;
    }

    public int AvailableSpotCount
    {
        get
        {
            var n = 0;
            foreach (var s in Spots)
            {
                var occupied = false;
                foreach (var c in _cars)
                {
                    if (s.IsOccupiedBy(c.AxisAlignedBounds))
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied)
                    n++;
            }

            return n;
        }
    }

    public void TryParkCurrentCar()
    {
        if (Phase != GamePhase.Playing || ActiveCar == null || ActiveCar.IsParked)
            return;

        ActiveCar.IsParked = true;
        if (_cars.Count < MaxCars)
        {
            var next = CreateCarAtSpawn(_cars.Count);
            _cars.Add(next);
            ActiveCar = next;
        }
        else
        {
            ActiveCar = null;
        }
    }

    public void Update(float deltaSeconds, bool left, bool right, bool up, bool down)
    {
        if (Phase != GamePhase.Playing)
            return;

        ActiveCar?.Drive(deltaSeconds, left, right, up, down, PlayArea);

        if (ActiveCar == null)
            return;

        foreach (var other in _cars)
        {
            if (ReferenceEquals(other, ActiveCar))
                continue;
            if (CarCollision.Intersects(ActiveCar, other))
            {
                LastCrashKind = CrashKind.CarCollision;
                Phase = GamePhase.Crashed;
                return;
            }
        }

        if (ActiveCar.WallImpactThisFrame)
        {
            LastCrashKind = CrashKind.Wall;
            Phase = GamePhase.Crashed;
        }
    }

    private void LayoutSpotsAndPlayArea()
    {
        var totalWidth = 4 * SpotWidth;
        var startX = (ClientWidth - totalWidth) / 2f;
        var startY = Margin + 48f;

        Spots = new ParkingSpot[4];
        for (var i = 0; i < 4; i++)
        {
            var x = startX + i * SpotWidth;
            var spotRect = new RectangleF(x, startY, SpotWidth, SpotHeight);
            var ledX = x + SpotWidth / 2f;
            var ledY = startY + SpotHeight + 14f;
            Spots[i] = new ParkingSpot(spotRect, new PointF(ledX, ledY));
        }

        PlayArea = new RectangleF(
            Margin,
            Margin,
            ClientWidth - 2 * Margin,
            ClientHeight - 2 * Margin);

        var carTopLeftY = startY + SpotHeight + AisleBelowSpots;
        _spawnCenterY = carTopLeftY + CarLengthPhysics / 2f;
    }

    private Car CreateCarAtSpawn(int colorIndex)
    {
        var heading = MathF.PI / 2f;
        var color = CarFlashColors[Math.Clamp(colorIndex, 0, CarFlashColors.Length - 1)];

        var hw = CarWidth * 0.5f;
        var hh = CarLengthPhysics * 0.5f;
        var halfDiag = MathF.Sqrt(hw * hw + hh * hh);

        var cx = PlayArea.Left + halfDiag + 12f;
        var cy = _spawnCenterY;

        var car = new Car(cx, cy, CarWidth, CarLengthCollision, CarLengthPhysics, heading, color);
        car.EnsureInside(PlayArea);
        return car;
    }
}
