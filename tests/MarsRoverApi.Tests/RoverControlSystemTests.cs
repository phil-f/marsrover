using Xunit;
using MarsRoverApi.Models;
using MarsRoverApi.ControlSystem;

namespace MarsRoverApi.Tests;

public class RoverControlSystemTests
{
    [Fact]
    public void RoverControlSystem_DeploymentPlan()
    {
        var firstRoverPosition = new Position(1, 2, Orientation.North);
        var firstRoverInstructions = new List<Instruction>
        {
            Instruction.Left,
            Instruction.Forward,
            Instruction.Left,
            Instruction.Forward,
            Instruction.Left,
            Instruction.Forward,
            Instruction.Left,
            Instruction.Forward,
            Instruction.Forward
        };
        var firstDeploymentCommand = new RoverDeploymentCommand(new Rover(1, firstRoverPosition), firstRoverInstructions);
        var firstRoverJourney = new List<Position>
        {
            new(1, 2, Orientation.North),
            new(1, 2, Orientation.West),
            new(0, 2, Orientation.West),
            new(0, 2, Orientation.South),
            new(0, 1, Orientation.South),
            new(0, 1, Orientation.East),
            new(1, 1, Orientation.East),
            new(1, 1, Orientation.North),
            new(1, 2, Orientation.North),
            new(1, 3, Orientation.North)
        };
        
        var secondRoverPosition = new Position(3, 3, Orientation.East);
        var secondRoverInstructions = new List<Instruction>
        {
            Instruction.Forward,
            Instruction.Forward,
            Instruction.Right,
            Instruction.Forward,
            Instruction.Forward,
            Instruction.Right,
            Instruction.Forward,
            Instruction.Right,
            Instruction.Right,
            Instruction.Forward
        };
        var secondDeploymentCommand = new RoverDeploymentCommand(new Rover(2, secondRoverPosition), secondRoverInstructions);
        var secondRoverJourney = new List<Position>
        {
            new(3, 3, Orientation.East),
            new(4, 3, Orientation.East),
            new(5, 3, Orientation.East),
            new(5, 3, Orientation.South),
            new(5, 2, Orientation.South),
            new(5, 1, Orientation.South),
            new(5, 1, Orientation.West),
            new(4, 1, Orientation.West),
            new(4, 1, Orientation.North),
            new(4, 1, Orientation.East),
            new(5, 1, Orientation.East)
        };

        var input = new RoverDeploymentInfo(new Plateau(5, 5), new List<RoverDeploymentCommand> { firstDeploymentCommand, secondDeploymentCommand });
        var controlSystem = new RoverControlSystem();
        var deploymentPlan = controlSystem.GenerateDeploymentPlan(input);
        var journeys = deploymentPlan.RoverJourneys;

        Assert.Equal(2, journeys.Count);
        Assert.Equal(firstRoverJourney, deploymentPlan.RoverJourneys[0].Positions);
        Assert.Equal(secondRoverJourney, deploymentPlan.RoverJourneys[1].Positions);
    }

    [Fact]
    public void RoverControlSystem_Collision()
    {
        var firstRoverPosition = new Position(0, 2, Orientation.East);
        var firstRoverInstructions = new List<Instruction> { Instruction.Forward, Instruction.Forward };
        var firstDeploymentCommand = new RoverDeploymentCommand(new Rover(1, firstRoverPosition), firstRoverInstructions);

        var secondRoverPosition = new Position(2, 2, Orientation.North);
        var secondRoverInstructions = new List<Instruction>();
        var secondDeploymentCommand = new RoverDeploymentCommand(new Rover(2, secondRoverPosition), secondRoverInstructions);

        var input = new RoverDeploymentInfo(new Plateau(5, 5), new List<RoverDeploymentCommand> { firstDeploymentCommand, secondDeploymentCommand });
        var controlSystem = new RoverControlSystem();
        var ex = Assert.Throws<Exception>(() => controlSystem.GenerateDeploymentPlan(input));

        Assert.Equal(
                "Collision detected when generating deployment plan. " +
                $"Rover 2 at position X: 2, Y: 2, Orientation: North will collide with " +
                $"Rover 1 at position X: 2, Y: 2, Orientation: East.", ex.Message);
    }

    [Theory]
    [InlineData(new[] { Instruction.Left, Instruction.Forward, Instruction.Forward }, "Rover 1 will go out of bounds at position X: -1, Y: 1, Orientation: West.")]
    [InlineData(new[] { Instruction.Forward }, "Rover 1 will go out of bounds at position X: 1, Y: 2, Orientation: North.")]
    public void RoverControlSystem_OutOfBounds(IEnumerable<Instruction> roverInstructions, string errorMessage)
    {
        var roverPosition = new Position(1, 1, Orientation.North);
        var deploymentCommand = new RoverDeploymentCommand(new Rover(1, roverPosition), roverInstructions);
        var input = new RoverDeploymentInfo(new Plateau(2, 1), new List<RoverDeploymentCommand> { deploymentCommand });
        var controlSystem = new RoverControlSystem();
        var ex = Assert.Throws<Exception>(() => controlSystem.GenerateDeploymentPlan(input));

        Assert.Equal(errorMessage, ex.Message);
    }
}
