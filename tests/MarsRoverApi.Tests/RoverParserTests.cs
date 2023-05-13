using Xunit;
using MarsRoverApi.Models;
using MarsRoverApi.Parsing;

namespace MarsRoverApi.Tests;

public class RoverParserTests
{
    [Fact]
    public void RoverParser_ParseRovers()
    {
        var input =
            "1 2 N|LMLMLMLMM" +
            Environment.NewLine +
            "3 3 E|MMRMMRMRRM";

        var parser = new RoverParser();
        using var reader = new StringReader(input);
        var result = parser.Parse(reader);
        var commands = result.RoverDeploymentCommands;

        Assert.Equal(2, commands.Count);

        var firstCommand = new RoverDeploymentCommand(new Rover(1, new Position(1, 2, Orientation.North)), new List<Instruction>
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
        });

        Assert.Equivalent(commands[0], firstCommand);

        var secondCommand = new RoverDeploymentCommand(new Rover(2, new Position(3, 3, Orientation.East)), new List<Instruction>
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
        });

        Assert.Equivalent(commands[1], secondCommand);
    }

    [Theory]
    [InlineData("1 2 N,LMLMLMLMM", "Unable to parse text '1 2 N,LMLMLMLMM'. Line number 1.", "Expected exactly 2 tokens when splitting line by delimiter '|'.")]
    [InlineData("1 2N|LMLMLMLMM", "Unable to parse text '1 2N|LMLMLMLMM'. Line number 1.", "Expected exactly 3 tokens when splitting position string by delimiter ' '.")]
    [InlineData("1 2 N|LMX", "Unable to parse text '1 2 N|LMX'. Line number 1.", "Unable to parse given instruction 'X'.")]
    [InlineData("1 2 D|LML", "Unable to parse text '1 2 D|LML'. Line number 1.", "Unable to parse given orientation 'D'.")]
    [InlineData("A 2 N|RLL", "Unable to parse text 'A 2 N|RLL'. Line number 1.", "Unable to parse given X coordinate 'A' to integer.")]
    [InlineData("1 B N|MRL", "Unable to parse text '1 B N|MRL'. Line number 1.", "Unable to parse given Y coordinate 'B' to integer.")]
    public void RoverParser_InvalidInput(string input, string message, string innerException)
    {
        var parser = new RoverParser();
        using var reader = new StringReader(input);
        var ex = Assert.Throws<Exception>(() => parser.Parse(reader));

        Assert.Equal(message, ex.Message);
        Assert.Equal(innerException, ex.InnerException?.Message);
    }

    [Fact]
    public void RoverParser_InvalidInputMultiline()
    {
        var parser = new RoverParser();
        var input =
            "1 2 E|LMR" +
            Environment.NewLine +
            "3 3 X|MLM";
        using var reader = new StringReader(input);
        var ex = Assert.Throws<Exception>(() => parser.Parse(reader));

        Assert.Equal("Unable to parse text '3 3 X|MLM'. Line number 2.", ex.Message);
        Assert.Equal("Unable to parse given orientation 'X'.", ex.InnerException?.Message);
    }
}
