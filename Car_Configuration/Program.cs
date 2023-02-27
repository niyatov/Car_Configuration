using Car_Configuration.Extensions;
using Car_Configuration.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Newtonsoft.Json;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddNewtonsoftJson(o =>
{
    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

builder.Services.AddAppDbContext(builder.Configuration);
builder.Services.AddIdentityManagers();
builder.SerilogConfig();

builder.Services.AddFluentValidationAutoValidation(o =>
{
    o.DisableDataAnnotationsValidation = false;
});

builder.Services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(Program)));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Signin";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandlerMiddleware("/Home/CustomError");
}

app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await Seed.InitializeRolesAsync(app);
await Seed.InitializeUserAsync(app);

app.Run();
