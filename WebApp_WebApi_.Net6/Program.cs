using Microsoft.AspNetCore.Authorization;
using System;
using WebApp_.Net6.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication("MyCookieAuthJulian").AddCookie("MyCookieAuthJulian", options =>
{
    options.Cookie.Name = "MyCookieAuthJulian";
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BelongHumanResources", policy =>
    {
        policy.RequireClaim("Department", "HR")
        .Requirements.Add(new HRManagerProbationRequirements(3));
    });

    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireClaim("Admin")
        .Requirements.Add(new HRManagerProbationRequirements(3));
    });

    options.AddPolicy("DevelopersOnly", policy =>
    {
        policy.RequireClaim("Developer");
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, HRManagerProbationRequirementsHandler>();

builder.Services.AddRazorPages();

builder.Services.AddHttpClient("OurWebAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:44323/");
});

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();
