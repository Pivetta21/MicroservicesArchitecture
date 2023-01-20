using Common.RabbitMq;
using Game.Data;
using Game.IoC;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Information()
             .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
             .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
             .MinimumLevel.Override("Game", LogEventLevel.Information)
             .WriteTo.Console()
             .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("\n====================================================================");
Console.WriteLine($"Name: {AppDomain.CurrentDomain.FriendlyName} #{Guid.NewGuid()}");
Console.WriteLine($"Host: {Environment.MachineName}");
Console.WriteLine("====================================================================\n");

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        options.UseAllOfForInheritance();
        options.UseOneOfForPolymorphism();

        options.SelectSubTypesUsing(baseType =>
            typeof(Program).Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType))
        );
    }
);

builder.Services.AddDbContext<GameDbContext>(optionsBuilder =>
    {
        var conn = builder.Configuration.GetConnectionString("GameDb");
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
