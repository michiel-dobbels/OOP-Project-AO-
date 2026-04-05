using System.Drawing;
using ParkingGarage.Models;

namespace ParkingGarage;

public enum GamePhase
{
    Playing,
    Crashed
}

public class ParkingGame
{
    public const int MaxCars = 5;

    private const float SpotWidth = 110f;
    private const float SpotHeight = 170f;
    private const float SpotGap = 24f;
    private const float CarWidth = 28f;
    private const float CarLength = 52f;
    private const float Margin = 40f;
    private const float AisleBelowSpots = 100f;

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

    public Car? ActiveCar { get; private set; }

    public IReadOnlyList<Car> Cars => _cars;

    public ParkingSpot[] Spots { get; private set; } = [];

    public RectangleF PlayArea { get; private set; }

    public void Resize(int width, int height)
    {
        ClientWidth = width;
        ClientHeight = height;
        LayoutSpotsAndPlayArea();
        ResetSimulation();
    }

    public void ResetSimulation()
    {
        Phase = GamePhase.Playing;
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
                Phase = GamePhase.Crashed;
                return;
            }
        }
    }

    private void LayoutSpotsAndPlayArea()
    {
        var totalWidth = 4 * SpotWidth + 3 * SpotGap;
        var startX = (ClientWidth - totalWidth) / 2f;
        var startY = Margin + 48f;

        Spots = new ParkingSpot[4];
        for (var i = 0; i < 4; i++)
        {
            var x = startX + i * (SpotWidth + SpotGap);
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
        _spawnCenterY = carTopLeftY + CarLength / 2f;
    }

    private Car CreateCarAtSpawn(int colorIndex)
    {
        var cx = PlayArea.Left + CarWidth / 2f + 14f;
        var heading = MathF.PI / 2f;
        var color = CarFlashColors[Math.Clamp(colorIndex, 0, CarFlashColors.Length - 1)];
        return new Car(cx, _spawnCenterY, CarWidth, CarLength, heading, color);
    }
}
