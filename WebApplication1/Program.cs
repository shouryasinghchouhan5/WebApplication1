using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApplication1.Data;
using WebApplication1.Services;
using WebApplication1.Models;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<NexusPayablesService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();

// Add Configuration singleton explicitly
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Enhanced logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventLog();

// Add DbContext with enhanced error handling and diagnostics
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);

        // Add command timeout
        sqlOptions.CommandTimeout(30);

        // Enable detailed errors (only in Development)
        if (builder.Environment.IsDevelopment())
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        }
    });

    // Log connection string (excluding sensitive data)
    var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
    if (!string.IsNullOrEmpty(connectionStringBuilder.Password))
        connectionStringBuilder.Password = "******";
    logger.LogInformation("Database connection string: {ConnectionString}", connectionStringBuilder.ToString());
});

// Health checks including database
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));

// Rest of your existing service configurations
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://example.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});
builder.Services.AddScoped<IDynamicFormService, DynamicFormService>();
// Add this line in your Program.cs where other services are registered
builder.Services.AddScoped<IDynamicFormService, DynamicFormService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Add health check endpoint
app.MapHealthChecks("/health");

// Global error handler
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (SqlException ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "SQL Server Error: {Message}", ex.Message);
        throw; // Re-throw to be handled by the exception handler middleware
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Your existing route configurations
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Kamson Properties routes
app.MapControllerRoute(
    name: "kamsonProperties",
    pattern: "kamson-properties",
    defaults: new { controller = "KamsonProperties", action = "Index" });

app.MapControllerRoute(
    name: "propertyListing",
    pattern: "kamson-properties/property-listing",
    defaults: new { controller = "KamsonProperties", action = "InHousePropertyListing" });

app.MapControllerRoute(
    name: "propertyAdmin",
    pattern: "kamson-properties/admin",
    defaults: new { controller = "KamsonProperties", action = "PropertyAdminDetails" });

app.MapControllerRoute(
    name: "bareApartment",
    pattern: "kamson-properties/bare",
    defaults: new { controller = "KamsonProperties", action = "BAREApartmentProfile" });

app.MapControllerRoute(
    name: "nexusIBS",
    pattern: "nexus-ibs",
    defaults: new { controller = "NexusIBS", action = "Index" });

app.MapControllerRoute(
    name: "accounting",
    pattern: "accounting",
    defaults: new { controller = "Accounting", action = "Index" });

app.Run();