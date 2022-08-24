using Microsoft.Extensions.Hosting.WindowsServices;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);
builder.Host.UseWindowsService();

var app = builder.Build();

app.MapGet("/", () =>
{
    return "FMP";
});

app.Run();
