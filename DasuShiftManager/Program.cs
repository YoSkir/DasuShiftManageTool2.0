using DasuShiftManager.Code;
using DasuShiftManager.Code.Init;
using DasuShiftManager.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//註冊生命週期
builder.Services.AddTransient<ShiftCreateTool>();
builder.Services.AddTransient<VacationDataGetter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

//初始化
using(var scope=app.Services.CreateScope())
{
    var initService = scope.ServiceProvider.GetRequiredService<InitService>();
    await initService.Init();
}

app.Run();
