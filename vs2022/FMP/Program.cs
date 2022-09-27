using Microsoft.Extensions.Hosting.WindowsServices;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);
builder.Host.UseWindowsService();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();

app.MapGet("/", () =>
{
    string reply = "";
    try
    {
        reply = FMP.WorkerManager.GetStatus();
    }
    catch (Exception ex)
    {
        reply = ex.Message;
    }
    return reply;
});

app.MapGet("/reboot", () =>
{
    string reply = "";
    try
    {
        FMP.WorkerManager.Restart();
        reply = "OK";
    }
    catch (Exception ex)
    {
        reply = ex.Message;
    }
    return reply;
});

app.MapGet("/upgrade", () =>
{
    string reply = "";
    try
    {
        FMP.WorkerManager.Upgrade();
        reply = "OK";
    }
    catch (Exception ex)
    {
        reply = ex.Message;
    }
    return reply;
});

FMP.WorkerManager.Start();
app.Run();
FMP.WorkerManager.Stop();
