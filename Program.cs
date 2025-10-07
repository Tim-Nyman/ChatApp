using ChatApp.Components;
using ChatApp.Data.Services;
using ChatApp.Hubs;
using ChatApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var cosmosSection = builder.Configuration.GetSection("CosmosDB");

builder.Services.AddSingleton(sp =>
    new CosmosService(
        cosmosSection["Endpoint"],
        cosmosSection["PrimaryKey"],
        cosmosSection["Database"]       
    )
);

var jwtSection = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ChatService>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseHttpsRedirection();
app.MapHub<ChatHub>("/chatHub");
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();