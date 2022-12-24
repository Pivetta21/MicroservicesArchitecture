using Game.Data;
using Game.IoC;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddServices();

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
