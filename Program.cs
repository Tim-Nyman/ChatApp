using ChatApp.Components;
using ChatApp.Data.Services;
using ChatApp.Hubs;
using ChatApp.Services;

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