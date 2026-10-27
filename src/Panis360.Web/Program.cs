using Panis360.Web;
using Panis360.Web.Components;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddHttpClient("Api",c=>c.BaseAddress=new Uri(builder.Configuration["ApiUrl"]??"http://localhost:5207"));
var app=builder.Build();
if(!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/erro");app.UseHsts();app.UseHttpsRedirection(); }
app.UseStaticFiles();app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();app.Run();
