using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
namespace WebApplication1.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PropertyService> _logger;
        public PropertyService(ApplicationDbContext context, ILogger<PropertyService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<PropertyListingViewModel>> GetInHousePropertyListing()
        {
            try
            {
                _logger.LogInformation("Starting to fetch property listing");
                // First check PropertyBasics table
                var testQuery = await _context.PropertyBasics.FirstOrDefaultAsync();
                if (testQuery == null)
                {
                    _logger.LogWarning("No records found in PropertyBasics table");
                    return new List<PropertyListingViewModel>();
                }
                try
                {
                    var properties = await (from pb in _context.PropertyBasics
                                            where pb.Active == true
                                            select new PropertyListingViewModel
                                            {
                                                PropertyID = pb.PropertyID,
                                                PropertyName = pb.PropertyName,
                                                MRI_PropertyID = pb.MRI_PropertyID ?? "N/A" // Handle null values
                                            }).ToListAsync();
                    _logger.LogInformation($"Basic query successful, found {properties.Count} properties");
                    return properties;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving property listing: {Message}", ex.Message);
                    // Additional logging or error handling logic
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error details: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
                }
                throw new Exception("An error occurred while retrieving the property listing", ex);
            }
        }
    }
}