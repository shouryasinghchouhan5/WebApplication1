using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication1.Services;
using WebApplication1.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class KamsonPropertiesController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IDynamicFormService _dynamicFormService;
        private readonly ILogger<KamsonPropertiesController> _logger;
        private readonly IConfiguration _configuration;

        public KamsonPropertiesController(
            IPropertyService propertyService,
            IDynamicFormService dynamicFormService,
            ILogger<KamsonPropertiesController> logger,
            IConfiguration configuration)
        {
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            _dynamicFormService = dynamicFormService ?? throw new ArgumentNullException(nameof(dynamicFormService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IActionResult Index()
        {
            try
            {
                _logger.LogInformation("Accessing Kamson Properties Index");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index");
                ViewBag.ErrorMessage = "An error occurred while accessing the index page.";
                return View();
            }
        }

        [HttpGet]
        [Route("KamsonProperties/test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                _logger.LogInformation("Starting database connection test");
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    ViewBag.Status = "Error";
                    ViewBag.Message = "Connection string is not configured";
                    return View("TestConnection");
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    _logger.LogInformation("Attempting to open connection");
                    connection.Open();

                    using (var command = new SqlCommand("SELECT DB_NAME()", connection))
                    {
                        var databaseName = command.ExecuteScalar()?.ToString();

                        ViewBag.Status = "Success";
                        ViewBag.Message = "Connected successfully!";
                        ViewBag.ServerName = connection.DataSource;
                        ViewBag.DatabaseName = databaseName;
                        ViewBag.ServerVersion = connection.ServerVersion;
                        ViewBag.State = connection.State.ToString();
                        ViewBag.ConnectionString = HideConnectionStringPassword(connectionString);

                        _logger.LogInformation("Connection test successful: {ServerName}, {DatabaseName}",
                            connection.DataSource, databaseName);
                    }
                }
                return View("TestConnection");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL connection test failed: {ErrorNumber}, {Message}", ex.Number, ex.Message);
                ViewBag.Status = "Error";
                ViewBag.Message = ex.Message;
                ViewBag.ErrorNumber = ex.Number;
                ViewBag.ErrorState = ex.State;
                ViewBag.ErrorClass = ex.Class;
                ViewBag.ServerName = ex.Server;
                ViewBag.ConnectionString = HideConnectionStringPassword(_configuration.GetConnectionString("DefaultConnection"));
                return View("TestConnection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed with unexpected error");
                ViewBag.Status = "Error";
                ViewBag.Message = ex.Message;
                ViewBag.ExceptionType = ex.GetType().Name;
                ViewBag.ConnectionString = HideConnectionStringPassword(_configuration.GetConnectionString("DefaultConnection"));
                return View("TestConnection");
            }
        }

        [HttpGet]
        [Route("KamsonProperties/InHousePropertyListing")]
        public async Task<IActionResult> InHousePropertyListing()
        {
            try
            {
                _logger.LogInformation("Starting to fetch In-House Property Listing using DynamicFormService");

                List<PropertyListingViewModel> properties;
                try
                {
                    properties = _dynamicFormService.GetPropertyListing();
                    _logger.LogInformation("Successfully retrieved {Count} properties using DynamicFormService",
                        properties?.Count ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get data using DynamicFormService, falling back to PropertyService");
                    properties = await _propertyService.GetInHousePropertyListing();
                }

                if (properties == null)
                {
                    _logger.LogWarning("Both services returned null");
                    ViewBag.ErrorMessage = "Unable to retrieve property listing data.";
                    return View(new List<PropertyListingViewModel>());
                }

                if (!properties.Any())
                {
                    _logger.LogInformation("No properties found in the listing");
                    ViewBag.Message = "No properties found.";
                    return View(new List<PropertyListingViewModel>());
                }

                properties = properties.OrderBy(p => p.PropertyID).ToList();

                _logger.LogInformation("Successfully retrieved and processed {Count} properties", properties.Count);
                return View(properties);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error occurred while fetching property listing: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner SQL exception: {Message}", ex.InnerException.Message);
                }
                ViewBag.ErrorMessage = $"Database error: {ex.Message}";
                return View(new List<PropertyListingViewModel>());
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error: {Message}", dbEx.Message);
                if (dbEx.InnerException != null)
                {
                    _logger.LogError(dbEx.InnerException, "Inner database exception: {Message}", dbEx.InnerException.Message);
                }
                ViewBag.ErrorMessage = $"Database error: {dbEx.Message}";
                return View(new List<PropertyListingViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching property listing");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
                }
                ViewBag.ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                return View(new List<PropertyListingViewModel>());
            }
        }
        [HttpGet]
        [Route("KamsonProperties/PersonnelReport")]
        public async Task<IActionResult> PersonnelReport()
        {
            try
            {
                _logger.LogInformation("Starting PersonnelReport action");
                var properties = await _propertyService.GetPersonnelReport();

                if (properties == null)
                {
                    _logger.LogWarning("Properties is null");
                    return View(new List<PropertyListingViewModel>());
                }

                _logger.LogInformation($"Retrieved {properties.Count} properties");

                // Log first few records for debugging
                foreach (var prop in properties.Take(3))
                {
                    _logger.LogInformation("Sample property: {@Property}", new
                    {
                        prop.EntityID,
                        prop.PropertyID,
                        prop.StaffAcct,
                        prop.AR,
                        prop.AP
                    });
                }

                return View(properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PersonnelReport action");
                throw;
            }
        }

        [HttpGet]
        [Route("KamsonProperties/PropertyAdminDetails")]
        public IActionResult PropertyAdminDetails()
        {
            try
            {
                _logger.LogInformation("Accessing Property Admin Details");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing Property Admin Details");
                ViewBag.ErrorMessage = "An error occurred while accessing Property Admin Details.";
                return View();
            }
        }

        [HttpGet]
        [Route("KamsonProperties/BAREApartmentProfile")]
        public IActionResult BAREApartmentProfile()
        {
            try
            {
                _logger.LogInformation("Accessing BARE Apartment Profile");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing BARE Apartment Profile");
                ViewBag.ErrorMessage = "An error occurred while accessing BARE Apartment Profile.";
                return View();
            }
        }

        private string HideConnectionStringPassword(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return null;
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder.Password))
                builder.Password = "******";
            return builder.ToString();
        }
    }
}