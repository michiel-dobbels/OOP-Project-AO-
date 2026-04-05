using System.Drawing;

namespace ParkingGarage.Models;

/// <summary>
/// One parking bay with white-line bounds and an LED at the entry side.
/// </summary>
public class ParkingSpot
{
    public ParkingSpot(RectangleF spotBounds, PointF ledCenter)
    {
        SpotBounds = spotBounds;
        LedCenter = ledCenter;
    }

    public RectangleF SpotBounds { get; }
    public PointF LedCenter { get; }

    /// <summary>
    /// True if any part of the car overlaps the spot (LED turns red).
    /// </summary>
    public bool IsOccupiedBy(RectangleF carBounds) => SpotBounds.IntersectsWith(carBounds);
}
