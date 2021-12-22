using CameraStream;
using CameraStream.Configuration;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var config = builder.Configuration;

var settings = new AppSettings();
config.Bind(settings);

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

app.MapRazorPages();

app.Run();
