using MarsRoverApi.ControlSystem;
using MarsRoverApi.Models;
using MarsRoverApi.Parsing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<RoverControlSystem>();
builder.Services.AddTransient<RoverParser>();
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(opts =>
{
    opts
      .AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
});
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/deployment-plan", (RoverDeploymentInfo input, RoverControlSystem controlSystem)
    => controlSystem.GenerateDeploymentPlan(input));

app.MapPost("/parse-rovers", (HttpContext ctx, RoverParser parser) =>
{
    var form = ctx.Request.Form;
    var file = form.Files.Single();
    using var reader = new StreamReader(file.OpenReadStream());
    var result = parser.Parse(reader);

    return result;
});

app.MapPost("/parse-rovers-debug", async (HttpContext ctx, RoverParser parser) =>
{
    using var streamReader = new StreamReader(ctx.Request.Body);
    var input = await streamReader.ReadToEndAsync();
    using var reader = new StringReader(input);
    var result = parser.Parse(reader);

    return result;
})
    .Accepts<string>("text/plain");

app.Run();
