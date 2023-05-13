using MarsRoverApi.Models;

namespace MarsRoverApi.Parsing;

public record RoverParsingResult(List<RoverDeploymentCommand> RoverDeploymentCommands);
