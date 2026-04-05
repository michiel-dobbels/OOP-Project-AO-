using System.Drawing;
using System.Numerics;

namespace ParkingGarage.Models;

/// <summary>SAT test for two oriented car rectangles (same OBB model as <see cref="Car"/>).</summary>
public static class CarCollision
{
    public static bool Intersects(Car a, Car b)
    {
        Span<PointF> ca = stackalloc PointF[4];
        Span<PointF> cb = stackalloc PointF[4];
        a.GetWorldCorners(ca);
        b.GetWorldCorners(cb);

        for (var i = 0; i < 4; i++)
        {
            var axis = PerpendicularAxis(ca[i], ca[(i + 1) % 4]);
            if (!OverlapsOnAxis(ca, cb, axis))
                return false;
        }

        for (var i = 0; i < 4; i++)
        {
            var axis = PerpendicularAxis(cb[i], cb[(i + 1) % 4]);
            if (!OverlapsOnAxis(ca, cb, axis))
                return false;
        }

        return true;
    }

    private static Vector2 PerpendicularAxis(PointF p0, PointF p1)
    {
        var dx = p1.X - p0.X;
        var dy = p1.Y - p0.Y;
        var len = MathF.Sqrt(dx * dx + dy * dy);
        if (len < 1e-5f)
            return new Vector2(0f, 1f);
        return new Vector2(-dy / len, dx / len);
    }

    private static void Project(ReadOnlySpan<PointF> poly, Vector2 axis, out float min, out float max)
    {
        min = float.PositiveInfinity;
        max = float.NegativeInfinity;
        foreach (var p in poly)
        {
            var n = p.X * axis.X + p.Y * axis.Y;
            if (n < min) min = n;
            if (n > max) max = n;
        }
    }

    private static bool OverlapsOnAxis(ReadOnlySpan<PointF> a, ReadOnlySpan<PointF> b, Vector2 axis)
    {
        Project(a, axis, out var a0, out var a1);
        Project(b, axis, out var b0, out var b1);
        return !(a1 < b0 || b1 < a0);
    }
}
