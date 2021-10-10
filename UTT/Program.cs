global using UTT;

using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddSignalR(o => o.EnableDetailedErrors = true);

builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

builder.Services.AddAuthentication("Cookies")
                .AddCookie()
                .AddTwitter(
                    options => builder.Configuration.GetSection("twitter").Bind(options));
                //.AddGoogle(
                //    options => Configuration.GetSection("google").Bind(options));

builder.Services.AddHostedService<Scavenger>();

// TODO: Design something less static 
// Initialize game settings from configuration
Game.Initialize(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<UTTHub>("/utt");

app.Run();