using E_Commerce.Data;
using E_Commerce.Hubs;
using E_Commerce.IRepository;
using E_Commerce.Middlewares;
using E_Commerce.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Add services to the container.
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // Configure Database
    builder.Services.AddDbContext<EcommerceDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // Register Services
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
    builder.Services.AddScoped<IProductRepo, ProductRepo>();
    builder.Services.AddScoped<ICartRepo, CartRepo>();
    builder.Services.AddScoped<ICartItemRepo, CartItemRepo>();
    builder.Services.AddScoped<IOrderRepo, OrderRepo>();
    builder.Services.AddScoped<IAuthRepo, AuthRepo>();
    builder.Services.AddSignalR();

    // Configure Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = builder.Configuration["AppSettings:Issuer"],
               ValidAudience = builder.Configuration["AppSettings:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!))
           };
       });

    // Ensure wwwroot directory exists
    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    if (!Directory.Exists(webRootPath))
    {
        Directory.CreateDirectory(webRootPath);
        Directory.CreateDirectory(Path.Combine(webRootPath, "images"));
        Directory.CreateDirectory(Path.Combine(webRootPath, "images", "categories"));
        Directory.CreateDirectory(Path.Combine(webRootPath, "images", "products"));
    }

    // Configure static files and web root
    builder.WebHost.UseWebRoot(webRootPath);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // Enable error handling first
    app.UseCustomErrorHandling();

    // Enable static files before other middleware
    app.UseStaticFiles();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<StockHub>("/stockHub");

    // Optional: Enable directory browsing (useful for development)
    if (app.Environment.IsDevelopment())
    {
        app.UseDirectoryBrowser();
    }

    app.Run();
}
catch (Exception ex)
{
    // Log the startup error
    var logger = LoggerFactory.Create(config =>
    {
        config.AddConsole();
        config.AddDebug();
    }).CreateLogger("Program");

    logger.LogError(ex, "Application startup failed");
    throw; // Rethrow to maintain the original error
}
