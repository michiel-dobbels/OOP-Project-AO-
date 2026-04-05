using System.Drawing;

namespace ParkingGarage.Models;

/// <summary>
/// Top-down car: center position, heading (clockwise from screen-up), forward/back drive and steer.
/// </summary>
public class Car
{
    public Car(float centerX, float centerY, float width, float height, float headingRadians = 0f)
    {
        CenterX = centerX;
        CenterY = centerY;
        Width = width;
        Height = height;
        HeadingRadians = headingRadians;
    }

    public float CenterX { get; set; }
    public float CenterY { get; set; }
    public float Width { get; }
    public float Height { get; }

    /// <summary>Clockwise radians from screen-up (nose toward smaller Y).</summary>
    public float HeadingRadians { get; set; }

    public float Speed { get; set; } = 200f;
    public float RotateSpeed { get; set; } = 2.85f;

    /// <summary>Axis-aligned bounds for overlap tests with parking spots.</summary>
    public RectangleF AxisAlignedBounds => ComputeAxisAlignedBounds(CenterX, CenterY);

    /// <summary>Degrees clockwise from up, for GDI+ <see cref="System.Drawing.Graphics.RotateTransform"/>.</summary>
    public float HeadingDegreesClockwiseFromUp => HeadingRadians * (180f / MathF.PI);

    private static bool IsFullyInside(RectangleF inner, RectangleF outer) =>
        inner.Left >= outer.Left &&
        inner.Right <= outer.Right &&
        inner.Top >= outer.Top &&
        inner.Bottom <= outer.Bottom;

    private RectangleF ComputeAxisAlignedBounds(float cx, float cy)
    {
        var hw = Width * 0.5f;
        var hh = Height * 0.5f;
        var corners = new (float lx, float ly)[]
        {
            (-hw, -hh), (hw, -hh), (hw, hh), (-hw, hh)
        };

        var minX = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var minY = float.PositiveInfinity;
        var maxY = float.NegativeInfinity;

        var c = MathF.Cos(HeadingRadians);
        var s = MathF.Sin(HeadingRadians);

        foreach (var (lx, lyGdi) in corners)
        {
            var lyFwd = -lyGdi;
            var wx = cx + lx * c + lyFwd * s;
            var wy = cy + lx * s - lyFwd * c;
            minX = MathF.Min(minX, wx);
            maxX = MathF.Max(maxX, wx);
            minY = MathF.Min(minY, wy);
            maxY = MathF.Max(maxY, wy);
        }

        return RectangleF.FromLTRB(minX, minY, maxX, maxY);
    }

    public void Drive(
        float deltaSeconds,
        bool turnLeft,
        bool turnRight,
        bool forward,
        bool backward,
        RectangleF playArea)
    {
        if (turnLeft)
            HeadingRadians -= RotateSpeed * deltaSeconds;
        if (turnRight)
            HeadingRadians += RotateSpeed * deltaSeconds;

        var move = 0f;
        if (forward)
            move += Speed * deltaSeconds;
        if (backward)
            move -= Speed * 0.55f * deltaSeconds;
        if (move == 0f)
            return;

        var fh = HeadingRadians;
        var vx = MathF.Sin(fh) * move;
        var vy = -MathF.Cos(fh) * move;

        var ncx = CenterX + vx;
        var ncy = CenterY + vy;

        var newBounds = ComputeAxisAlignedBounds(ncx, ncy);
        if (IsFullyInside(newBounds, playArea))
        {
            CenterX = ncx;
            CenterY = ncy;
            return;
        }

        var bx = ComputeAxisAlignedBounds(ncx, CenterY);
        if (IsFullyInside(bx, playArea))
            CenterX = ncx;

        var by = ComputeAxisAlignedBounds(CenterX, ncy);
        if (IsFullyInside(by, playArea))
            CenterY = ncy;
    }
}
