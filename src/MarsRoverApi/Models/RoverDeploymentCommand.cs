namespace MarsRoverApi.Models;

public record RoverDeploymentCommand(Rover Rover, IEnumerable<Instruction> Instructions);
