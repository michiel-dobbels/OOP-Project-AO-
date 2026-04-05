using System.Drawing;
using ParkingGarage.Models;

namespace ParkingGarage;


public class ParkingGame
{
    private const float SpotWidth = 110f;
    private const float SpotHeight = 170f;
    private const float SpotGap = 24f;
    private const float CarWidth = 28f;
    private const float CarLength = 52f;
    private const float Margin = 40f;
    private const float AisleBelowSpots = 100f;

    public ParkingGame(int clientWidth, int clientHeight)
    {
        ClientWidth = clientWidth;
        ClientHeight = clientHeight;
        RebuildLayout();
    }

    public int ClientWidth { get; private set; }
    public int ClientHeight { get; private set; }

    public Car Car { get; private set; } = null!;
    public ParkingSpot[] Spots { get; private set; } = [];

    public RectangleF PlayArea { get; private set; }

    public void Resize(int width, int height)
    {
        ClientWidth = width;
        ClientHeight = height;
        RebuildLayout();
    }

    public int AvailableSpotCount
    {
        get
        {
            var b = Car.AxisAlignedBounds;
            var n = 0;
            foreach (var s in Spots)
            {
                if (!s.IsOccupiedBy(b))
                    n++;
            }
            return n;
        }
    }

    public void Update(float deltaSeconds, bool left, bool right, bool up, bool down)
    {
        Car.Drive(deltaSeconds, left, right, up, down, PlayArea);
    }

    private void RebuildLayout()
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

        var carTopLeftX = PlayArea.Left + (PlayArea.Width - CarWidth) / 2f;
        var carTopLeftY = startY + SpotHeight + AisleBelowSpots;
        var cx = carTopLeftX + CarWidth / 2f;
        var cy = carTopLeftY + CarLength / 2f;
        Car = new Car(cx, cy, CarWidth, CarLength, headingRadians: 0f);
    }
}
