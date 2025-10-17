using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.API.Data;
using LibraryManagement.API.Repositories;
using LibraryManagement.API.Services;
using LibraryManagement.API.Configurations;
using FluentValidation.AspNetCore;
using FluentValidation;
using LibraryManagement.API.Validators;
using LibraryManagement.API.Mappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

Env.Load(); // ✅ Giữ ở đây, ok

var builder = WebApplication.CreateBuilder(args);

// Get connection string from env
var conn = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
           builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

// Add services to the container.
builder.Services.AddSwaggerGen();

// AutoMapper
builder.Services.AddAutoMapper(config => config.AddMaps(typeof(Program).Assembly));

// ✅ CORS for React frontend
builder.Services.AddCorsConfiguration();

// Register repositories and services
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<AuthorRepository>();
builder.Services.AddScoped<AuthorService>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BorrowingRepository>();
builder.Services.AddScoped<BorrowingService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddValidatorsFromAssemblyContaining<BookValidator>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Middleware: error handler
app.UseMiddleware<LibraryManagement.API.Middlewares.ErrorHandlingMiddleware>();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Đặt CORS đúng vị trí
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


//xin chao 