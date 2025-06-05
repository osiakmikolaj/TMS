using Microsoft.AspNetCore.Authentication.Cookies;
using TMS.Application;
using TMS.Infrastructure;
using TMS.RazorPages.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdministratorRole");
});

// Dodaj uwierzytelnianie cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Dodaj autoryzacjê z rolami
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy =>
        policy.RequireRole("Administrator"));
    options.AddPolicy("RequireDeveloperRole", policy =>
        policy.RequireRole("Developer"));
});

// Dodaj warstwy aplikacji i infrastruktury
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Dodaj klienta gRPC
builder.Services.AddSingleton<GrpcTicketClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();