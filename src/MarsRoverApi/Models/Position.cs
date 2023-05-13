namespace MarsRoverApi.Models;

public record Position(int X, int Y, Orientation Orientation)
{
    public override string ToString()
        => $"X: {X}, Y: {Y}, Orientation: {Orientation}";
}
