using CameraStream;
using CameraStream.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var config = builder.Configuration;

var settings = new AppSettings();
config.Bind(settings);

services.AddControllers().AddControllersAsServices();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cameras", Version = "v1" });
});

services.AddSingleton<IEnumerable<CameraSettings>>(svc => settings.Cameras);
services.AddSingleton<CameraServer>();

services.AddHostedService<CameraUpdateService>();

services.AddCors();

// Add services to the container.
services.AddRazorPages();

    var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cameras v1"));

app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
