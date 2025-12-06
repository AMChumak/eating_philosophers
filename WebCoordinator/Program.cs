using System.Reflection;
using Publisher;
using WebCoordinator;

await Host.CreateDefaultBuilder(args)
    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions<RabbitMQSettings>().Bind(hostContext.Configuration.GetSection("RabbitMQ"));

        services.AddSingleton<IPublisher, RabbitMQPublisher>();

        services.AddHostedService<MessageProcessor>();
    })
    .RunConsoleAsync();