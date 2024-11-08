using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class PropertyListingViewModel
    {
        // Primary identifiers
        [Display(Name = "Property ID")]
        public string PropertyID { get; set; }  // Changed to string to match database

        [Display(Name = "MRI Property ID")]
        public string? MRI_PropertyID { get; set; }

        [Display(Name = "Property Name")]
        public string PropertyName { get; set; }

        // Address information
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        // Contact information
        public string Email { get; set; }
        [Display(Name = "Phone")]
        public string PropTel { get; set; }
        public string Fax { get; set; }

        // Property details
        [Display(Name = "Number of Units")]
        public int? NoOfUnits { get; set; }
        [Display(Name = "Tax ID")]
        public string TaxID { get; set; }
        [Display(Name = "Date Purchased")]
        [DataType(DataType.Date)]
        public DateTime? DatePurchased { get; set; }

        // Location details
        public string County { get; set; }
        public string Borough { get; set; }
        public string Township { get; set; }
        [Display(Name = "School District")]
        public string SchoolDistrict { get; set; }

        // Management information
        [Display(Name = "Property Manager")]
        public string PropMgr { get; set; }
        [Display(Name = "Co-Property Manager")]
        public string CoPropMgr { get; set; }
        [Display(Name = "Administrative Assistant")]
        public string AdmAsst { get; set; }

        // Entity information
        [Display(Name = "Entity ID")]
        public string EntityID { get; set; }  // Changed to string to match database
        [Display(Name = "Entity Name")]
        public string EntityName { get; set; }
        public string DBA { get; set; }

        // Financial information
        [Display(Name = "Staff Account")]
        public string StaffAcct { get; set; }
        public string AR { get; set; }
        public string AP { get; set; }
        [Display(Name = "AP CAPEX")]
        public string AP_CAPEX { get; set; }

        // Site management
        [Display(Name = "Onsite Manager")]
        public string OnsiteMgr { get; set; }
        [Display(Name = "OM Beeper")]
        public string OMBeeper { get; set; }
        public string Supt { get; set; }
        [Display(Name = "Supt Tel")]
        public string SuptTel { get; set; }
        [Display(Name = "Supt Beeper")]
        public string SuptBeeper { get; set; }

        // Formatted fields
        [Display(Name = "City, State Zip")]
        public string CSZ { get; set; }
        [Display(Name = "AR Memo")]
        public string ARMemo { get; set; }
        [Display(Name = "Construction Manager")]
        public string ConstructionMgr { get; set; }
    }
}