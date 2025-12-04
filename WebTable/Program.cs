using PhilosopherLib;
using StatisticsLib;
using NetworkContracts;
using ForkLib;
using SimulationSettingsLib;
using WebTable;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddOptions<SimulationSettings>().Bind(builder.Configuration.GetSection("Simulation"));

builder.Services.AddSingleton<ITableManager, NetTableManager>();
builder.Services.AddSingleton<IStatistics, Statistics>();


Dictionary<string, NetPhilosopher> philosophers = [];

var app = builder.Build();

var logger = app.Services.GetService<ILogger>();

var statistics = app.Services.GetService<IStatistics>();
var cts = new CancellationTokenSource();
var statisticsToken = cts.Token;

var tableManager = app.Services.GetService<ITableManager>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/forks/take", async (HttpContext context) =>
    {
        try
        {
            var request = await JsonSerializer.DeserializeAsync<TakeForkRequest>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request body");
                return;
            }

            var response = new TakeForkResponse
            {
                IsSuccess = philosophers[request.PhilosopherName].TakeFork(request.ForkId)
            };

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Error: {ex.Message}");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Error: {ex.Message}");
        }
    });

app.MapPost("/forks/release", async (HttpContext context) =>
    {
        try
        {
            var request = await JsonSerializer.DeserializeAsync<ReleaseForksRequest>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request body");
                return;
            }

            philosophers[request.PhilosopherName].ReleaseFork(request.ForkId);

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Fork released successfully");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Error: {ex.Message}");
        }
    });

app.MapPost("/philosopher/enter", async (HttpContext context) =>
    {
        try
        {
            var request = await JsonSerializer.DeserializeAsync<PhilosopherEnterRequest>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request body");
                return;
            }
            int seat = tableManager!.TakeSeat(request.PhilosopherName);
            NetPhilosopher netPhilosopher= new NetPhilosopher(request.PhilosopherName, tableManager.GetLeftFork(seat), tableManager.GetRightFork(seat));
            statistics!.AddPhilosopher(netPhilosopher);

            philosophers.Add(request.PhilosopherName, netPhilosopher);

            var response = new PhilosopherEnterResponse
            {
                LeftFork = netPhilosopher.LeftFork.OrderNumber,
                RightFork = netPhilosopher.RightFork.OrderNumber
            };

            if (philosophers.Count >= 5)
            {
                _ = statistics!.Overwatch(statisticsToken);
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Error: {ex.Message}");
        }
    });

app.MapPost("/philosopher/exit", async (HttpContext context) =>
    {
        try
        {
            var request = await JsonSerializer.DeserializeAsync<PhilosopherExitRequest>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request body");
                return;
            }
            var philosopher = philosophers[request.PhilosopherName];

            if (philosopher.LeftFork.Owner == request.PhilosopherName)
            {
                philosopher.ReleaseFork(philosopher.LeftFork);
            }

            if (philosopher.RightFork.Owner == request.PhilosopherName)
            {
                philosopher.ReleaseFork(philosopher.RightFork);
            }

            philosophers.Remove(request.PhilosopherName);

            if (philosophers.Count == 0)
            {
                cts.Cancel();
                await Task.Delay(500);
                statistics!.PrintStatistics();
            }

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Exit successfully");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Error: {ex.Message}");
        }
    });

app.MapPost("/philosopher/update", async (HttpContext context) =>
    {
        try
        {
            var request = await JsonSerializer.DeserializeAsync<PhilosopherUpdateRequest>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request body");
                return;
            }

            var philosopher = philosophers[request.PhilosopherName];

            philosopher.UpdateState(request.NewState);

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Updated state successfully");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Error: {ex.Message}");
        }
    });


var port = Environment.GetEnvironmentVariable("PORT") ?? "5050";
app.Run($"http://0.0.0.0:{port}");


