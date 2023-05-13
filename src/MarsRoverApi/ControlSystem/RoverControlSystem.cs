using MarsRoverApi.Models;

namespace MarsRoverApi.ControlSystem;

public class RoverControlSystem
{
    public RoverDeploymentPlan GenerateDeploymentPlan(RoverDeploymentInfo info)
    {
        var roverJourneys = new List<RoverJourney>();
        var stationedRovers = new List<Rover>();

        foreach (var command in info.RoverDeploymentCommands)
        {
            var rover = command.Rover;
            var instructions = command.Instructions;
            var positions = new List<Position> { rover.Position };
            var currentPosition = positions.Single();

            ValidatePosition(rover.Id, currentPosition, stationedRovers, info.Plateau);

            foreach (var instruction in instructions)
            {
                currentPosition = GetNextPosition(currentPosition, instruction);
                ValidatePosition(rover.Id, currentPosition, stationedRovers, info.Plateau);
                positions.Add(currentPosition);
            }

            roverJourneys.Add(new RoverJourney(rover.Id, positions));
            stationedRovers.Add(rover with { Position = currentPosition });
        }

        var plan = new RoverDeploymentPlan(roverJourneys);

        return plan;
    }

    private static Position GetNextPosition(
        Position currentPosition, Instruction instruction)
    {
        return instruction switch
        {
            Instruction.Right => RotateRight(currentPosition),
            Instruction.Left => RotateLeft(currentPosition),
            Instruction.Forward => MoveForwards(currentPosition),
            _ => throw new Exception($"No handling registered for given instruction: {instruction}.")
        };
    }

    private static Position MoveForwards(Position position)
    {
        var orientation = position.Orientation;
        var nextPosition = orientation switch
        {
            Orientation.North => position with { Y = position.Y + 1 },
            Orientation.South => position with { Y = position.Y - 1 },
            Orientation.East => position with { X = position.X + 1 },
            Orientation.West => position with { X = position.X - 1 },
            _ => throw new Exception($"No handling registered for given orientation '{orientation}'.")
        };

        return nextPosition;
    }

    private static void ValidatePosition(int roverId, Position position, IEnumerable<Rover> stationedRovers, Plateau plateau)
    {
        AssertPositionWithinBounds(roverId, position, plateau);
        AssertPositionIsAvailable(stationedRovers, roverId, position);
    }
    
    private static void AssertPositionWithinBounds(int roverId, Position position, Plateau plateau)
    {
        if (position.Y < 0 || position.Y > plateau.Height ||
            position.X < 0 || position.X > plateau.Width)
        {
            throw new Exception($"Rover {roverId} will go out of bounds at position {position}.");
        }
    }

    private static void AssertPositionIsAvailable(
        IEnumerable<Rover> stationedRovers, int roverId, Position position)
    {
        var obstructingRover =
            stationedRovers.FirstOrDefault(sr =>
                sr.Position.X == position.X && sr.Position.Y == position.Y);

        if (obstructingRover != null)
        {
            throw new Exception(
                "Collision detected when generating deployment plan. " +
                $"Rover {roverId} at position {position} will collide with " +
                $"Rover {obstructingRover.Id} at position {obstructingRover.Position}.");
        }
    }

    private static Position RotateLeft(Position position)
    {
        var currentOrientation = position.Orientation;
        var nextOrientation = currentOrientation switch
        {
            Orientation.North => Orientation.West,
            Orientation.South => Orientation.East,
            Orientation.East => Orientation.North,
            Orientation.West => Orientation.South,
            _ => throw new Exception($"No handling registered for given orientation '{currentOrientation}'.")
        };

        return position with { Orientation = nextOrientation };
    }

    private static Position RotateRight(Position position)
    {
        var currentOrientation = position.Orientation;
        var nextOrientation = currentOrientation switch
        {
            Orientation.North => Orientation.East,
            Orientation.South => Orientation.West,
            Orientation.East => Orientation.South,
            Orientation.West => Orientation.North,
            _ => throw new Exception($"No handling registered for given orientation '{currentOrientation}'.")
        };

        return position with { Orientation = nextOrientation };
    }
}
