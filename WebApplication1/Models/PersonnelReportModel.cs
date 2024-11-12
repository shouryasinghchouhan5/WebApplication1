using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class PersonnelReportModel
    {
        public string EntityID { get; set; }
        public string PropertyID { get; set; }
        public string MRI_PropertyID { get; set; }
        [Display(Name = "Property Name")]
        public string PropertyName { get; set; }
        [Display(Name = "# of Units")]
        public int? NoOfUnits { get; set; }
        [Display(Name = "Property Manager")]
        public string PropMgr { get; set; }
        [Display(Name = "Admin Assistant")]
        public string AdmAsst { get; set; }
        [Display(Name = "Staff Acct")]
        public string StaffAcct { get; set; }
        public string AR { get; set; }
        public string AP { get; set; }
    }
}