using Microsoft.EntityFrameworkCore;
using Radzen;
using SignalF.Measurement.Viewer;
using SignalF.Measurement.Viewer.Components;
using SignalF.Measurement.Viewer.Data;
using Microsoft.AspNetCore.Identity;
using SignalF.Measurement.Viewer.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.Components.Authorization;

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
builder.Services.AddHttpClient("SignalF.Measurement.Viewer").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false }).AddHeaderPropagation(o => o.Headers.Add("Cookie"));
builder.Services.AddHeaderPropagation(o => o.Headers.Add("Cookie"));
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddScoped<SignalF.Measurement.Viewer.SecurityService>();
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SignalFDbConnection"));
});
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ApplicationIdentityDbContext>().AddDefaultTokenProviders();
builder.Services.AddControllers().AddOData(o =>
{
    var oDataBuilder = new ODataConventionModelBuilder();
    oDataBuilder.EntitySet<ApplicationUser>("ApplicationUsers");
    var usersType = oDataBuilder.StructuralTypes.First(x => x.ClrType == typeof(ApplicationUser));
    usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.Password)));
    usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.ConfirmPassword)));
    oDataBuilder.EntitySet<ApplicationRole>("ApplicationRoles");
    o.AddRouteComponents("odata/Identity", oDataBuilder.GetEdmModel()).Count().Filter().OrderBy().Expand().Select().SetMaxTop(null).TimeZone = TimeZoneInfo.Utc;
});
builder.Services.AddScoped<AuthenticationStateProvider, SignalF.Measurement.Viewer.ApplicationAuthenticationStateProvider>();
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
app.UseHeaderPropagation();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>().Database.Migrate();
app.Run();