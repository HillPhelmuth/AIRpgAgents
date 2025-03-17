using AIRpgAgents.Components;
using AIRpgAgents.Core.Models;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Cosmos;
using Radzen;
using AIRpgAgents.ChatModels;
using AIRpgAgents.Core.Services;
using SkPluginComponents.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
services.AddRazorComponents()
    .AddInteractiveServerComponents().AddHubOptions(o =>
    {
        o.MaximumReceiveMessageSize = null;
    }); 
var cosmosClient = new CosmosClient(builder.Configuration["Cosmos:ConnectionString"]);
Config.LoadConfig(builder.Configuration);
services.AddSingleton(cosmosClient);
services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"]!;
    options.ClientId = builder.Configuration["Auth0:ClientId"]!;
});
services.AddCascadingAuthenticationState();
services.AddRadzenComponents();
services.AddAIRpgAgentsCore();
services.AddScoped<ChatStateCollection>().AddTransient<AppJsInterop>();
services.AddSingleton<ICharacterCreationService, CharacterCreationService>();
services.AddRollDisplayService();
//builder.Services.AddScoped<ICharacterService, CharacterService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapGet("/account/login", async (HttpContext httpContext, string redirectUri = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
        .WithRedirectUri(redirectUri)
        .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/account/logout", async (HttpContext httpContext, string redirectUri = "/") =>
{
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
        .WithRedirectUri(redirectUri)
        .Build();

    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
