using CatalogService.Api.Extensions;
using CatalogService.Api.Infrastructure.Setup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.Configure<CatalogSettings>(builder.Configuration.GetSection("CatalogSettings"));
//builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureConsul(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//app.MigrateDbContext<CatalogContext>((context, services) =>
//{
//    var env = services.GetService<IWebHostEnvironment>();
//    var logger = services.GetService<ILogger<CatalogContextSeed>>();

//    new CatalogContextSeed().SeedAsync(context, env, logger).Wait();
//});

//app.UseWebRoot("pics");
//app.UseContentRoot(Directory.GetCurrentDirectory());

app.MapControllers();

app.RegisterWithConsul(lifetime: app.Lifetime);

app.Run();
