using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.Settings
{
    public class BiometricExcelSettingVM
    {
        // ===================== BASIC EXCEL MAPPING =====================
        public int Id { get; set; }

        public string? StatusName { get; set; }
        public string? InTimeName { get; set; }
        public string? OutTimeName { get; set; }
        public string? TotalWorkedHrs { get; set; }
        public string? ReferenceInTime { get; set; } // "09:30"

        public int InTimeNameStartRow { get; set; }
        public int InTimeDataStartCol { get; set; }
        public int EmpCodeIndexFromInTime { get; set; }

        public int StatusNameIndexFromInTime { get; set; }
        public int OutTimeIndexFromInTime { get; set; }
        public int TotalWorkHrsIndexFromInTime { get; set; }
        public int EmpIdColIndex { get; set; }
        public int HeaderInTimeStartCol { get; set; }

        public string? PrefixBfrID { get; set; }
        public string? SplCharAfterID { get; set; }

        public string? OTName { get; set; }
        public int OTNameIndexFromInTime { get; set; }

        // ===================== STATUS NAME REFERENCES =====================
        public string? P1 { get; set; }
        public string? P2 { get; set; }
        public string? H { get; set; }
        public string? W1 { get; set; }
        public string? M1 { get; set; }
        public string? M2 { get; set; }
        public string? N { get; set; }
        public string? A { get; set; }
        public string? L { get; set; }

        // ===================== PF (DISPLAY ONLY) =====================
        public bool PF_Basic { get; set; }
        public bool PF_HRA { get; set; }
        public bool PF_Convey { get; set; }
        public bool PF_MiscEarning { get; set; }
        public bool PF_MedicalAllowance { get; set; }
        public bool PF_EA { get; set; }
        public bool PF_DA { get; set; }
        public bool PF_LTA { get; set; }

        // ===================== OT (DISPLAY ONLY) =====================
        public bool OT_Basic { get; set; }
        public bool OT_DA { get; set; }
        public bool OT_HRA { get; set; }
        public bool OT_Convey { get; set; }
        public bool OT_SpecialAllowance { get; set; }
        public bool OT_MedicalAllowance { get; set; }
        public bool OT_LTA { get; set; }
        public bool OT_EA { get; set; }
        public bool OT_MiscEarning { get; set; }
        public bool OT_OtherAllowance { get; set; }
        public bool OT_AttBonus { get; set; }

        // ===================== ESI (DISPLAY ONLY) =====================
        public bool ESI_Basic { get; set; }
        public bool ESI_DA { get; set; }
        public bool ESI_HRA { get; set; }
        public bool ESI_Convey { get; set; }
        public bool ESI_OTAmount { get; set; }
        public bool ESI_SpecialAllowance { get; set; }
        public bool ESI_ManualAttendance { get; set; }
        public bool ESI_LTA { get; set; }
        public bool ESI_EA { get; set; }
        public bool ESI_MiscEarning { get; set; }
        public bool ESI_OtherAllowance { get; set; }
        public bool ESI_AttBonus { get; set; }

        // ============= ESI SALARY (DISPLAY ONLY) =========
        public string? EmployerESICode { get; set; }
        public string? EmployeeESI { get; set; }
        public string? EmployerESI { get; set; }
        public string? ESISealing { get; set; } 
        public string? EmployerESIPercentage { get; set; }

        // =========== PF SALARY (DISPLAY ONLY) ==========
        public string? EmployerCodeNo { get; set; }
        public string? EmployerPFAc1 { get; set; }
        public string? EmployerAcGroupNo { get; set; }
        public string? EmployerPFAc10 { get; set; }
        public string? EmployeePF { get; set; }
        public string? EmployerPFAc21 { get; set; }
        public string? PFSealing { get; set; }
        public string? AdminChrAc2 { get; set; }
        public string? AdminChrAc22 { get; set; }
    }
}
