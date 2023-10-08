using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(config =>
{
    config.AddEntityFrameworkOutbox<AuctionDbContext>(options =>
    {
        options.QueryDelay = TimeSpan.FromSeconds(10); // if the service bus is available, the message will be delivered
        // immediately. If it's not, every 10 seconds it's going to attempt to look inside our outbox and see if there's anything
        // that hasn't been delivered yet once the service bus is available.

        options.UsePostgres();
        options.UseBusOutbox();
    });

    config.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    config.UsingRabbitMq((context, cfg) => { cfg.ConfigureEndpoints(context); });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();