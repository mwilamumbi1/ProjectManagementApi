using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementApi.DataContext;
using ProjectManagementApi.Services;
using QuestPDF.Infrastructure;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

QuestPDF.Settings.License = LicenseType.Community;

// Configure JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Register DbContext with SQL Server
builder.Services.AddDbContext<PMDataContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Fix: Register your services for dependency injection.
// This tells the application how to create instances of your services.
builder.Services.AddScoped<IUserService, UserService>();
 

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure CORS for your React application
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// Order of middleware is CRITICAL for authentication and authorization!
app.UseHttpsRedirection();
app.UseRouting(); // Identifies what endpoint is being hit

// UseCors MUST be after UseRouting and before UseAuthentication
app.UseCors("AllowReactApp");

app.UseAuthentication(); // THIS IS ESSENTIAL: Checks for the token and authenticates the user
app.UseAuthorization(); // THIS IS ESSENTIAL: Checks if the authenticated user has permissions

app.MapControllers(); // Maps incoming requests to controller actions

app.Run();
