using Core.Convertors;
using Core.Services;
using Core.Services.Interfaces;
using DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;


var builder = WebApplication.CreateBuilder(args);

#region Service
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

builder.Services.AddMvc(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IUserservice, Userservice>();
builder.Services.AddTransient<IViewRenderService, RenderViewToString>();
// builder.Services.AddTransient<IPermissionService, PermissionService>();
// builder.Services.AddTransient<IForumService, ForumService>();

#endregion

#region Context

builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductConnection"));
});

#endregion Context

#region Authentication

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(43200);
});

#endregion

#region Timing Login

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
});

#endregion Timing Login

var app = builder.Build();
// Configure the HTTP request pipeline.

#region Pipline

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

// app.MapGet("/home", () => "Hello!");
#region Rout

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "MyAreas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
    endpoints.MapControllerRoute(
        name: "Default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

#endregion Rout
app.MapDefaultControllerRoute();

app.MapRazorPages();

app.Run();

#endregion
