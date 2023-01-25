using Game.Data;
using Game.IoC;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "Game";

Console.WriteLine("\n====================================================================");
Console.WriteLine($"App: {serviceName} #{Guid.NewGuid()}");
Console.WriteLine($"Name: {AppDomain.CurrentDomain.FriendlyName}");
Console.WriteLine($"Host: {Environment.MachineName}");
Console.WriteLine("====================================================================\n");

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Information()
             .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
             .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
             .MinimumLevel.Override(serviceName, LogEventLevel.Information)
             .WriteTo.Console()
             .WriteTo.Elasticsearch(
                 options: new ElasticsearchSinkOptions(node: new Uri(builder.Configuration["Elasticsearch:Url"] ?? ""))
                 {
                     IndexFormat = $"microservices-{serviceName.ToLower()}-logs" +
                         $"-{builder.Environment.EnvironmentName.ToLower().Replace('.', '-')}" +
                         $"-{DateTime.UtcNow:yyyy-MM-dd}",
                     TemplateName = "microservices",
                     AutoRegisterTemplate = true,
                     OverwriteTemplate = true,
                     AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                     BatchAction = ElasticOpType.Create,
                     TypeName = null,
                 }
             )
             .Enrich.WithProperty("ServiceName", serviceName)
             .Enrich.WithProperty("InstanceName", AppDomain.CurrentDomain.FriendlyName)
             .Enrich.WithProperty("HostName", Environment.MachineName)
             .CreateLogger();


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
