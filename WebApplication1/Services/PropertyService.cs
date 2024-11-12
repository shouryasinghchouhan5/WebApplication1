using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PropertyService> _logger;
        private readonly IConfiguration _configuration;

        public PropertyService(
            ApplicationDbContext context,
            ILogger<PropertyService> logger,
            IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<PropertyListingViewModel>> GetInHousePropertyListing()
        {
            try
            {
                _logger.LogInformation("Starting to fetch property listing");
                var testQuery = await _context.PropertyBasics.FirstOrDefaultAsync();
                if (testQuery == null)
                {
                    _logger.LogWarning("No records found in PropertyBasics table");
                    return new List<PropertyListingViewModel>();
                }

                var properties = await (from pb in _context.PropertyBasics
                                        where pb.Active == true
                                        select new PropertyListingViewModel
                                        {
                                            PropertyID = pb.PropertyID,
                                            PropertyName = pb.PropertyName,
                                            MRI_PropertyID = pb.MRI_PropertyID ?? "N/A"
                                        }).ToListAsync();

                _logger.LogInformation($"Basic query successful, found {properties.Count} properties");
                return properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property listing");
                throw;
            }
        }

        public async Task<List<PropertyListingViewModel>> GetPersonnelReport()
        {
            try
            {
                _logger.LogInformation("Starting to fetch Personnel Report data");

                var query = @"
            SELECT 
                pb.EntityID,            -- For debugging exact column names
                pb.PropertyID,
                ISNULL(pb.MRI_PropertyID, 'N/A') as MRI_PropertyID,
                pb.PropertyName,
                ISNULL(pa.NoOfUnits, 0) as NoOfUnits,
                ISNULL(pa.PropMgr, '-') as PropMgr,
                CASE 
                    WHEN pa.AdmAsst IS NULL OR RTRIM(LTRIM(pa.AdmAsst)) = '' 
                    THEN 'None Assigned'
                    ELSE pa.AdmAsst 
                END as AdmAsst,
                ISNULL(sa.StaffAcct, '-') as StaffAcct,
                ISNULL(ar.AR, '-') as AR,
                ISNULL(ap.AP, '-') as AP
            FROM dbo.tblPropertyBasics pb
            LEFT JOIN dbo.tblPropertyAdmin pa ON pb.PropertyID = pa.PropertyID
            LEFT JOIN dbo.tblSA sa ON pb.EntityID = sa.EntityID
            LEFT JOIN dbo.tblAR ar ON pb.PropertyID = ar.PropertyID
            LEFT JOIN dbo.tblAP ap ON pb.PropertyID = ap.PropertyID
            WHERE pb.Active = 1
            ORDER BY pb.EntityID, pb.PropertyID";

                var properties = new List<PropertyListingViewModel>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Log column names
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                _logger.LogInformation($"Column {i}: {reader.GetName(i)}");
                            }

                            while (await reader.ReadAsync())
                            {
                                var property = new PropertyListingViewModel
                                {
                                    EntityID = reader["EntityID"]?.ToString(),
                                    PropertyID = reader["PropertyID"]?.ToString(),
                                    MRI_PropertyID = reader["MRI_PropertyID"]?.ToString(),
                                    PropertyName = reader["PropertyName"]?.ToString(),
                                    NoOfUnits = reader["NoOfUnits"] != DBNull.Value ?
                                        Convert.ToInt32(reader["NoOfUnits"]) : 0,
                                    PropMgr = reader["PropMgr"]?.ToString(),
                                    AdmAsst = reader["AdmAsst"]?.ToString(),
                                    StaffAcct = reader["StaffAcct"]?.ToString(),
                                    AR = reader["AR"]?.ToString(),
                                    AP = reader["AP"]?.ToString()
                                };

                                // Log each property's data
                                _logger.LogInformation("Row data: {@Property}", new
                                {
                                    property.EntityID,
                                    property.PropertyID,
                                    property.StaffAcct,
                                    property.AR,
                                    property.AP
                                });

                                properties.Add(property);
                            }
                        }
                    }
                }

                _logger.LogInformation($"Retrieved {properties.Count} properties");
                return properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPersonnelReport");
                throw;
            }
        }

        public async Task<List<PropertyListingViewModel>> GetBAREReport()
        {
            try
            {
                _logger.LogInformation("Starting to fetch BARE report data");

                var properties = await _context.PropertyBasics
                    .Where(p => p.Active)
                    .Select(p => new PropertyListingViewModel
                    {
                        PropertyID = p.PropertyID,
                        PropertyName = p.PropertyName,
                        MRI_PropertyID = p.MRI_PropertyID ?? "N/A"
                    })
                    .ToListAsync();

                _logger.LogInformation($"Successfully retrieved {properties.Count} properties for BARE report");
                return properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving BARE report data");
                throw;
            }
        }

        public async Task<List<PropertyListingViewModel>> GetPropertyAdminDetails()
        {
            try
            {
                _logger.LogInformation("Starting to fetch property admin details");

                var properties = await _context.PropertyBasics
                    .Where(p => p.Active)
                    .Select(p => new PropertyListingViewModel
                    {
                        PropertyID = p.PropertyID,
                        PropertyName = p.PropertyName,
                        MRI_PropertyID = p.MRI_PropertyID ?? "N/A"
                    })
                    .ToListAsync();

                _logger.LogInformation($"Successfully retrieved {properties.Count} property admin details");
                return properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property admin details");
                throw;
            }
        }
    }
}