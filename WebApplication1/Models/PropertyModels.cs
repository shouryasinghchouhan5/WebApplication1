using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("dbo.tblPropertyBasics")]
    public class PropertyBasics
    {
        [Key]
        public string PropertyID { get; set; }
        public string? MRI_PropertyID { get; set; }  // Made nullable
        public string PropertyName { get; set; }
        public string? EntityID { get; set; }        // Made nullable
        public string? TaxID { get; set; }           // Made nullable
        public DateTime? DatePurchased { get; set; }
        public bool Active { get; set; }
    }

    [Table("dbo.tblPropertyAdmin")]
    public class PropertyAdmin
    {
        [Key]
        public string PropertyID { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public string PropTel { get; set; }
        public string Fax { get; set; }
        public int? NoOfUnits { get; set; }
        public string County { get; set; }
        public string Borough { get; set; }
        public string Township { get; set; }
        public string SchoolDistrict { get; set; }
        public string PropMgr { get; set; }
        public string CoPropMgr { get; set; }
        public string AdmAsst { get; set; }
        public string OnsiteMgr { get; set; }
        public string OMBeeper { get; set; }
        public string Supt { get; set; }
        public string SuptTel { get; set; }
        public string SuptBeeper { get; set; }
    }

    [Table("dbo.tblEntity")]
    public class Entity
    {
        [Key]
        public string EntityID { get; set; }
        public string EntityName { get; set; }
        public string DBA { get; set; }
    }

    [Table("dbo.tblSA")]
    public class StaffAccount
    {
        [Key]
        public string EntityID { get; set; }
        public string StaffAcct { get; set; }
    }

    [Table("dbo.tblAR")]
    public class AccountsReceivable
    {
        [Key]
        public string PropertyID { get; set; }
        public string AR { get; set; }
        public string ARMemo { get; set; }
    }

    [Table("dbo.tblAP")]
    public class AccountsPayable
    {
        [Key]
        public string PropertyID { get; set; }
        public string AP { get; set; }
    }

    [Table("dbo.tblAPCapex")]
    public class APCapex
    {
        [Key]
        public string PropertyID { get; set; }
        public string AP_CAPEX { get; set; }
    }

    [Table("dbo.tblConsMgr")]
    public class ConstructionManager
    {
        [Key]
        public string PropertyID { get; set; }
        public string ConstructionMgr { get; set; }
    }
}