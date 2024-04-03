using Microsoft.EntityFrameworkCore;
using Radzen;
using SignalF.Measurement.Viewer;
using SignalF.Measurement.Viewer.Components;
using SignalF.Measurement.Viewer.Data;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient();
builder.Services.AddScoped<SignalFDbService>();
//builder.Services.AddDbContext<SignalF.Measurement.Viewer.Data.SignalFDbContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("SignalFDbConnection"));
//});
builder.Services.AddDbContextPool<SignalFDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SignalFDbConnection"));
});
builder.Services.AddScoped<SignalFDbService>();
builder.Services.AddDbContext<SignalF.Measurement.Viewer.Data.SignalFDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SignalFDbConnection"));
});
builder.Services.AddScoped<SignalF.Measurement.Viewer.SignalFDbService>();
builder.Services.AddLocalization();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseRequestLocalization(options => options.AddSupportedCultures("en", "de").AddSupportedUICultures("en", "de").SetDefaultCulture("en"));
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();