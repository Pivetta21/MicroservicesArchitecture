using Armory;
using Armory.Data;
using Armory.IoC;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

AppConfig.ShowApplicationInfo();

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Information()
             .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
             .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
             .MinimumLevel.Override(AppConfig.ServiceName, LogEventLevel.Information)
             .WriteTo.Console()
             .WriteTo.Elasticsearch(
                 options: new ElasticsearchSinkOptions(node: new Uri(builder.Configuration["Elasticsearch:Url"] ?? ""))
                 {
                     IndexFormat = $"{AppConfig.ServiceName.ToLower().Replace('.', '-')}-logs" +
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
             .Enrich.WithProperty("ServiceName", AppConfig.ServiceName)
             .Enrich.WithProperty("InstanceName", AppConfig.InstanceName)
             .Enrich.WithProperty("InstanceUuid", AppConfig.InstanceUuid)
             .Enrich.WithProperty("HostName", AppConfig.HostName)
             .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry()
       .WithTracing(tracerProviderBuilder =>
           {
               tracerProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(AppConfig.ServiceName));

               tracerProviderBuilder.AddAspNetCoreInstrumentation();

               tracerProviderBuilder.AddHttpClientInstrumentation(
                   options => options.FilterHttpRequestMessage =
                       httpRequestMessage =>
                       {
                           var requestHost = httpRequestMessage.RequestUri?.Host;
                           var requestPort = httpRequestMessage.RequestUri?.Port;
                           var requestUrl = $"{requestHost}:{requestPort}";

                           var elasticSearchUrl = builder.Configuration["Elasticsearch:Url"] ?? "";
                           return requestUrl.Contains(elasticSearchUrl);
                       }
               );

               tracerProviderBuilder.AddEntityFrameworkCoreInstrumentation();

               tracerProviderBuilder.AddSource(AppConfig.DungeonEntranceSource.Name);
               tracerProviderBuilder.AddSource(AppConfig.PlayDungeonSource.Name);

               tracerProviderBuilder.AddJaegerExporter(jaegerOptions =>
                   {
                       jaegerOptions.AgentHost = builder.Configuration["Jaeger:Host"];
                       jaegerOptions.AgentPort = int.Parse(builder.Configuration["Jaeger:Port"] ?? "");
                   }
               );

               // tracerProviderBuilder.AddConsoleExporter();
           }
       )
       .StartWithHost();

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

builder.Services.AddSyncDataServices();

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
