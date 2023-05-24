using MarsRoverApi.Models;

namespace MarsRoverApi.Parsing;

public class RoverParser
{
    public RoverParsingResult Parse(TextReader reader)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        var lineNumber = 0;
        string? line = null;
        var commands = new List<RoverDeploymentCommand>();

        try
        {
            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                lineNumber++;

                var tokens = GetTokens(line);
                var positionToken = tokens[0];
                var instructionsToken = tokens[1];
                var position = ParsePosition(positionToken);
                var instructions = ParseInstructions(instructionsToken);
                var roverId = lineNumber;
                var rover = new Rover(roverId, position);

                commands.Add(new RoverDeploymentCommand(rover, instructions));
            }

            var result = new RoverParsingResult(commands);

            return result;
        }
        catch(Exception ex)
        {
            throw new Exception(
                $"Unable to parse text '{line}'. Line number {lineNumber}.", ex);
        }
    }

    private static string[] GetTokens(string str)
    {
        const string delimiter = "|";
        var tokens = str.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length != 2)
        {
            throw new Exception(
                $"Expected exactly 2 tokens when splitting line by delimiter '{delimiter}'.");
        }

        return tokens;
    }

    private static Position ParsePosition(string str)
    {
        const string delimiter = " ";
        var tokens = str.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length != 3)
        {
            throw new Exception(
                $"Expected exactly 3 tokens when splitting position string by delimiter '{delimiter}'.");
        }

        var (x, y) = ParseCoordinates(tokens[0], tokens[1]);
        var orientation = ParseOrientation(tokens[2]);

        return new Position(x, y, orientation);
    }

    private static (int, int) ParseCoordinates(string x, string y)
    {
        if (!int.TryParse(x, out var xCoordinate))
        {
            throw new Exception($"Unable to parse given X coordinate '{x}' to integer.");
        }

        if (!int.TryParse(y, out var yCoordinate))
        {
            throw new Exception($"Unable to parse given Y coordinate '{y}' to integer.");
        }

        return (xCoordinate, yCoordinate);
    }

    private static Orientation ParseOrientation(string str)
    {
        return str switch
        {
            "N" => Orientation.North,
            "S" => Orientation.South,
            "W" => Orientation.West,
            "E" => Orientation.East,
            _ => throw new Exception($"Unable to parse given orientation '{str}'.")
        };
    }

    private static IList<Instruction> ParseInstructions(string instructions)
    {
        return instructions.Select(instruction => instruction.ToString() switch
        {
            "L" => Instruction.Left,
            "R" => Instruction.Right,
            "M" => Instruction.Forward,
            _ => throw new Exception($"Unable to parse given instruction '{instruction}'.")
        }).ToList();
    }
}
