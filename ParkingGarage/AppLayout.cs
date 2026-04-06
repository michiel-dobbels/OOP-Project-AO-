namespace ParkingGarage;

/// <summary>Single window client size: 55% larger than the original 960×600 simulation field.</summary>
public static class AppLayout
{
    public static readonly Size SimulationClientSize = new(
        (int)Math.Round(960 * 1.55),
        (int)Math.Round(600 * 1.55));
}
