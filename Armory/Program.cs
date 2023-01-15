using Armory.Data;
using Armory.IoC;
using Common.RabbitMq;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("\n====================================================================");
Console.WriteLine($"Name: {AppDomain.CurrentDomain.FriendlyName} #{Guid.NewGuid()}");
Console.WriteLine($"Host: {Environment.MachineName}");
Console.WriteLine("====================================================================\n");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ArmoryDbContext>(optionsBuilder =>
    {
        var conn = builder.Configuration.GetConnectionString("ArmoryDb");
        var username = builder.Configuration["POSTGRES_USER"];
        var password = builder.Configuration["POSTGRES_PASSWORD"];

        optionsBuilder
            .UseNpgsql(string.Format(conn!, username, password))
            .UseSnakeCaseNamingConvention();
    }
);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"],
        Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672"),
        DispatchConsumersAsync = true,
    }
);

builder.Services.AddSingleton<RabbitMqConnectionManager>();

builder.Services.AddServices();

builder.Services.AddAsyncDataServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

AppDbInitializer.SeedData(app);

app.Run();
