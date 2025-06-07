using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TMS.API.Middleware;
using TMS.Application;
using TMS.Infrastructure;
using TMS.Infrastructure.Persistence;


var builder = WebApplication.CreateBuilder(args);

// Dodaj us³ugi z warstw Application i Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Konfiguracja kontrolerów API
builder.Services.AddControllers();

// Konfiguracja Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdministratorRole");
});

// Konfiguracja us³ugi gRPC
builder.Services.AddGrpc();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

// Dodanie Swaggera
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TMS API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Konfiguracja uwierzytelniania JWT dla API
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "JWT_OR_COOKIE";
    options.DefaultChallengeScheme = "JWT_OR_COOKIE";
})
// Uwierzytelnianie za pomoc¹ JWT dla API
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "TMSDefaultSecretKey123!@#"))
    };
})
// Uwierzytelnianie za pomoc¹ ciasteczek dla Razor Pages
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
// Schemat, który wybiera miêdzy JWT a Cookie w zale¿noœci od kontekstu
.AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // SprawdŸ, czy ¿¹danie jest dla API (na podstawie œcie¿ki)
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }
        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
});

// Dodanie autoryzacji z rolami
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy =>
        policy.RequireRole("Administrator"));
    options.AddPolicy("RequireDeveloperRole", policy =>
        policy.RequireRole("Developer"));
});

// Dodanie klienta gRPC dla Razor Pages
builder.Services.AddScoped<TMS.RazorPages.Services.GrpcTicketClient>();

var app = builder.Build();

// Inicjalizacja bazy danych
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
    await initializer.InitialiseAsync();
    await initializer.SeedAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Dodanie middleware do obs³ugi nag³ówków HTTP
app.UseMiddleware<CustomHeadersMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Mapowanie endpointów
app.MapControllers();      // API Controllers
app.MapRazorPages();       // Razor Pages

// Mapowanie us³ug gRPC z u¿yciem aliasów przestrzeni nazw
app.MapGrpcService<TMS.GRPC.Services.GrpcTicketService>();
app.MapGrpcService<TMS.GRPC.Services.GreeterService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();