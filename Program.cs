using ChatApp.Components;
using ChatApp.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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