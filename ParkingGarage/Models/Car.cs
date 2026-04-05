using System.Drawing;

namespace ParkingGarage.Models;

/// <summary>
/// Top-down car: a simple rectangle with position and movement inside a play area.
/// </summary>
public class Car
{
    public Car(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; }
    public float Height { get; }

    public float Speed { get; set; } = 220f;

    public RectangleF Bounds => new(X, Y, Width, Height);

    private static bool IsFullyInside(RectangleF inner, RectangleF outer) =>
        inner.Left >= outer.Left &&
        inner.Right <= outer.Right &&
        inner.Top >= outer.Top &&
        inner.Bottom <= outer.Bottom;

    public void Move(float deltaX, float deltaY, float deltaSeconds, RectangleF playArea)
    {
        var nx = X + deltaX * Speed * deltaSeconds;
        var ny = Y + deltaY * Speed * deltaSeconds;

        var b = new RectangleF(nx, ny, Width, Height);
        if (IsFullyInside(b, playArea))
        {
            X = nx;
            Y = ny;
        }
        else
        {
            if (nx >= playArea.Left && nx + Width <= playArea.Right)
                X = nx;
            if (ny >= playArea.Top && ny + Height <= playArea.Bottom)
                Y = ny;
        }
    }
}
