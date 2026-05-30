using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartHealthcare.Application;
using SmartHealthcare.Application.Common.Settings;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Infrastructure;
using SmartHealthcare.Infrastructure.Authentication;
using SmartHealthcare.Persistence;
using SmartHealthcare.Persistence.Contexts;
using SmartHealthcare.Persistence.Seed;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add Controllers
builder.Services.AddControllers();

builder.Services.AddApplication();

builder.Services.AddInfrastucture(builder.Configuration);

builder.Services.AddPersistance(builder.Configuration);


// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

//JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));


var jwtsettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtsettings == null || string.IsNullOrEmpty(jwtsettings.Secret))
{
    throw new Exception("JWT Settings are missing in appsettings.json");
}
var key = Encoding.UTF8.GetBytes(jwtsettings.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtsettings.Issuer,
        ValidAudience = jwtsettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
        

    };
});

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        // Password
        options.Password.RequiredLength = 8;

        options.Password.RequireDigit = true;

        options.Password.RequireUppercase = true;

        options.Password.RequireLowercase = true;

        options.Password.RequireNonAlphanumeric = false;


        // Lockout
        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);


        // User
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


var app = builder.Build();


// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await RoleSeeder.SeedRolesAsync(services);

    await SuperAdminSeeder.SuperAdminSeederAsync(services);
}

app.Run();