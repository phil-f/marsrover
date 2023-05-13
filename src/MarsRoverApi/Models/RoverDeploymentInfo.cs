namespace MarsRoverApi.Models;

public record RoverDeploymentInfo(Plateau Plateau, List<RoverDeploymentCommand> RoverDeploymentCommands);
