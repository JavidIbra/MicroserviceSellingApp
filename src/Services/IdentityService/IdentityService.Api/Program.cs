using IdentityService.Api.Application.Services;
using IdentityService.Api.Extensions.Registration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IIdentityService , IdentityService.Api.Application.Services.IdentityService>();

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

app.ConfigureConsul(lifetime: app.Lifetime); 

app.MapControllers();

app.Run();
