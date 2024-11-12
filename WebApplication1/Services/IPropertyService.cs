using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IPropertyService
    {
        Task<List<PropertyListingViewModel>> GetInHousePropertyListing();
        Task<List<PropertyListingViewModel>> GetPersonnelReport();
        Task<List<PropertyListingViewModel>> GetBAREReport();
        Task<List<PropertyListingViewModel>> GetPropertyAdminDetails();
    }
}