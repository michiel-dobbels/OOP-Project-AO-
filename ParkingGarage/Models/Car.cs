using System.Drawing;

namespace ParkingGarage.Models;

/// <summary>
/// Top-down car: center position, heading (clockwise from screen-up), forward/back drive and steer.
/// </summary>
public class Car
{
    public Car(float centerX, float centerY, float width, float heightCollision, float heightVisual, float headingRadians, Color bodyColor)
    {
        CenterX = centerX;
        CenterY = centerY;
        Width = width;
        Height = heightCollision;
        VisualHeight = heightVisual;
        HeadingRadians = headingRadians;
        BodyColor = bodyColor;
    }

    public float CenterX { get; set; }
    public float CenterY { get; set; }
    public float Width { get; }
    /// <summary>Length along the body (long axis) for collision, movement bounds, and parking checks.</summary>
    public float Height { get; }

    /// <summary>Length for drawing (sprites / block body); can be larger than <see cref="Height"/>.</summary>
    public float VisualHeight { get; }

    /// <summary>Clockwise radians from screen-up (nose toward smaller Y).</summary>
    public float HeadingRadians { get; set; }

    public Color BodyColor { get; }

    /// <summary>Parked cars no longer move or accept input.</summary>
    public bool IsParked { get; set; }

    public float Speed { get; set; } = 190f;
    public float RotateSpeed { get; set; } = 2.35f;

    public RectangleF AxisAlignedBounds => ComputeAxisAlignedBounds(CenterX, CenterY);

    /// <summary>Set at end of <see cref="Drive"/> when throttle was held but the car could not move while touching the play-area edge.</summary>
    public bool WallImpactThisFrame { get; private set; }

    public float HeadingDegreesClockwiseFromUp => HeadingRadians * (180f / MathF.PI);

    public bool TouchesPlayAreaEdge(RectangleF playArea, float epsilon = 2.5f)
    {
        var b = AxisAlignedBounds;
        return b.Left <= playArea.Left + epsilon ||
               b.Right >= playArea.Right - epsilon ||
               b.Top <= playArea.Top + epsilon ||
               b.Bottom >= playArea.Bottom - epsilon;
    }

    public void GetWorldCorners(Span<PointF> destination)
    {
        var hw = Width * 0.5f;
        var hh = Height * 0.5f;
        ReadOnlySpan<(float lx, float lyGdi)> local =
        [
            (-hw, -hh), (hw, -hh), (hw, hh), (-hw, hh)
        ];

        var c = MathF.Cos(HeadingRadians);
        var s = MathF.Sin(HeadingRadians);

        for (var i = 0; i < 4; i++)
        {
            var (lx, lyGdi) = local[i];
            var lyFwd = -lyGdi;
            destination[i] = new PointF(
                CenterX + lx * c + lyFwd * s,
                CenterY + lx * s - lyFwd * c);
        }
    }

    private static bool IsFullyInside(RectangleF inner, RectangleF outer) =>
        inner.Left >= outer.Left &&
        inner.Right <= outer.Right &&
        inner.Top >= outer.Top &&
        inner.Bottom <= outer.Bottom;

    /// <summary>Nudges the center so the oriented AABB fits inside the play area (e.g. after spawn).</summary>
    public void EnsureInside(RectangleF playArea)
    {
        for (var i = 0; i < 6; i++)
        {
            var b = AxisAlignedBounds;
            if (IsFullyInside(b, playArea))
                return;

            if (b.Left < playArea.Left)
                CenterX += playArea.Left - b.Left;
            if (b.Right > playArea.Right)
                CenterX -= b.Right - playArea.Right;
            if (b.Top < playArea.Top)
                CenterY += playArea.Top - b.Top;
            if (b.Bottom > playArea.Bottom)
                CenterY -= b.Bottom - playArea.Bottom;
        }
    }

    private RectangleF ComputeAxisAlignedBounds(float cx, float cy)
    {
        var hw = Width * 0.5f;
        var hh = Height * 0.5f;
        Span<PointF> corners = stackalloc PointF[4];
        var c = MathF.Cos(HeadingRadians);
        var s = MathF.Sin(HeadingRadians);

        ReadOnlySpan<(float lx, float lyGdi)> local =
        [
            (-hw, -hh), (hw, -hh), (hw, hh), (-hw, hh)
        ];

        var minX = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var minY = float.PositiveInfinity;
        var maxY = float.NegativeInfinity;

        for (var i = 0; i < 4; i++)
        {
            var (lx, lyGdi) = local[i];
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
        if (IsParked)
            return;

        WallImpactThisFrame = false;

        var forwardOnly = forward && !backward;
        var backwardOnly = backward && !forward;
        var moving = forwardOnly || backwardOnly;

        if (moving)
        {
            var steerMul = backwardOnly ? -1f : 1f;
            if (turnLeft)
                HeadingRadians -= RotateSpeed * deltaSeconds * steerMul;
            if (turnRight)
                HeadingRadians += RotateSpeed * deltaSeconds * steerMul;
        }

        var move = 0f;
        if (forwardOnly)
            move += Speed * deltaSeconds;
        if (backwardOnly)
            move -= Speed * 0.55f * deltaSeconds;
        if (move == 0f)
            return;

        var startCx = CenterX;
        var startCy = CenterY;

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

        EnsureInside(playArea);

        var moved = MathF.Abs(CenterX - startCx) > 0.35f || MathF.Abs(CenterY - startCy) > 0.35f;
        if (!moved && TouchesPlayAreaEdge(playArea))
            WallImpactThisFrame = true;
    }
}
