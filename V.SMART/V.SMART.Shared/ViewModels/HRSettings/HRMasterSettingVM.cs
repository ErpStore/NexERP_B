using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.Settings
{
    public class HRMasterSettingVM
    {
        public int Id { get; set; }

        //#region ESI Settings

        public decimal EmplrESICode { get; set; }
        public decimal ESISealing { get; set; }
        public decimal EmpESI { get; set; }
        public decimal EmplrESI { get; set; }

        // Corresponds to old Misc.ESIBasicDA
        public bool ESIBasicDA { get; set; }

        //#endregion

        //#region PF Settings

        public decimal EmplrCodeNo { get; set; }
        public decimal EmprAcGroupNo { get; set; }

        public decimal EmpPF { get; set; }
        public decimal PFSealing { get; set; }

        public decimal EmplrPFAcNo { get; set; }
        public decimal EmplrPFAcNo21 { get; set; }

        public decimal AdminChrAcNo2 { get; set; }
        public decimal AdminChrAcNo22 { get; set; }

        //#endregion

        //#region PF Calculation Heads

        public bool PFBasic { get; set; }
        public bool PFDA { get; set; }
        public bool PFHRA { get; set; }
        public bool PFConveyance { get; set; }
        public bool PFOTAmount { get; set; }
        public bool PFSplAllow { get; set; }
        public bool PFMedicalAllow { get; set; }
        public bool PFLTA { get; set; }
        public bool PFEA { get; set; }
        public bool PFMiscEarning { get; set; }
        public bool PFOtherAllow { get; set; }
        public bool PFAttendBonus { get; set; }

        //#endregion

        //#region ESI Calculation Heads

        public bool ESIBasic { get; set; }
        public bool ESIDA { get; set; }
        public bool ESIHRA { get; set; }
        public bool ESIConveyance { get; set; }
        public bool ESIOTAmt { get; set; }
        public bool ESISplAllow { get; set; }
        public bool ESIMedicalAllow { get; set; }
        public bool ESILTA { get; set; }
        public bool ESIEA { get; set; }
        public bool ESIMiscEarning { get; set; }
        public bool ESIOtherAllow { get; set; }
        public bool ESIAttBonus { get; set; }

        //#endregion

        //#region OT Calculation Heads

        public bool OTBasic { get; set; }
        public bool OTDA { get; set; }
        public bool OTHRA { get; set; }
        public bool OTEA { get; set; }
        public bool OTLTA { get; set; }
        public bool OTSplAllow { get; set; }
        public bool OTConveyance { get; set; }
        public bool OTMedicalAllow { get; set; }
        public bool OTMiscEarning { get; set; }
        public bool OTOtherAllow { get; set; }
        public bool OTAttendBonus { get; set; }

        //#endregion

        //#region Helper Properties For Calculation

        public decimal PFPercent => EmpPF;

        public decimal PFMaxAmount => PFSealing;

        public decimal ESIPercent => EmpESI;

        public decimal ESIMaxAmount => ESISealing;

    }
}