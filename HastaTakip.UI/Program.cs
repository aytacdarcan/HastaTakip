using HastaTakip.UI.Components;
using HastaTakip.UI.Services;

var builder = WebApplication.CreateBuilder(args);

//  API base: https://localhost:7283/api/
builder.Services.AddHttpClient("HastaApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7283/api/"); 
});


builder.Services.AddScoped<HastaApi>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
