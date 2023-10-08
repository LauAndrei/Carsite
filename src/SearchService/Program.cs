using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(config =>
{
    config.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>(); // any consumer, any other consumer we create
    // in this namespace are automatically gonna be registered by mass transit

    config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            // we gonna try 5 times, and wait 5 seconds between each interval
            e.UseMessageRetry(r => r.Interval(5, 5));

            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

// without this register handler, the service wouldn't have started if the AuctionService wasn't started
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});

app.Run();

// this keep trying every 5 sec until AuctionService is back up and the call succeeds
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
{
    return HttpPolicyExtensions.HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(5));
}