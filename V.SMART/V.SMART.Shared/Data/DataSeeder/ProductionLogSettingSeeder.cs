using V.SMART.Shared.Data.GeneralSettingsMaster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.DataSeeder
{
     public static class ProductionLogSettingSeeder
        {
            public static void Seed(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ProductionLogSetting>().HasData(

                    // ========== CATEGORY A : NO EFFICIENCY LOSS (100%) ==========
                    new ProductionLogSetting { Id = 1, ProdStopType = "Testing", EfficiencyPerc = 100, Definition = "Time spent on part inspection, testing, quality checks.", IsActive = true },
                    new ProductionLogSetting { Id = 2, ProdStopType = "Machine Breakdown", EfficiencyPerc = 100, Definition = "Time lost due to machine failure or mechanical issues.", IsActive = true },
                    new ProductionLogSetting { Id = 3, ProdStopType = "Tool Shortage", EfficiencyPerc = 100, Definition = "Delay due to tools not available or not issued.", IsActive = true },
                    new ProductionLogSetting { Id = 4, ProdStopType = "New Program Setup", EfficiencyPerc = 100, Definition = "Machine stopped for loading and validating new NC program.", IsActive = true },
                    new ProductionLogSetting { Id = 5, ProdStopType = "Fixture Issue / No Fixture", EfficiencyPerc = 100, Definition = "Waiting for fixture correction, availability, or repair.", IsActive = true },
                    new ProductionLogSetting { Id = 6, ProdStopType = "Waiting for Skilled Operator", EfficiencyPerc = 100, Definition = "Operator not available due to workload distribution.", IsActive = true },
                    new ProductionLogSetting { Id = 7, ProdStopType = "Meeting / Training", EfficiencyPerc = 100, Definition = "Operator in meeting or undergoing training.", IsActive = true },
                    new ProductionLogSetting { Id = 8, ProdStopType = "Waiting for Load", EfficiencyPerc = 100, Definition = "Delay due to material movement or preparation.", IsActive = true },
                    new ProductionLogSetting { Id = 9, ProdStopType = "Chip Clearing", EfficiencyPerc = 100, Definition = "Routine chip removal to maintain cutting efficiency.", IsActive = true },
                    new ProductionLogSetting { Id = 10, ProdStopType = "Waiting for Crane", EfficiencyPerc = 100, Definition = "Delay for overhead crane or lifting equipment.", IsActive = true },
                    new ProductionLogSetting { Id = 11, ProdStopType = "Coolant Refill / Level Low", EfficiencyPerc = 100, Definition = "Machine stoppage for coolant refilling or circulation checks.", IsActive = true },
                    new ProductionLogSetting { Id = 12, ProdStopType = "Operator Absent", EfficiencyPerc = 100, Definition = "Shift absence resulting in machine idle time.", IsActive = true },
                    new ProductionLogSetting { Id = 13, ProdStopType = "Planned Maintenance", EfficiencyPerc = 100, Definition = "Scheduled maintenance activities.", IsActive = true },
                    new ProductionLogSetting { Id = 14, ProdStopType = "Power Failure", EfficiencyPerc = 100, Definition = "Grid or internal power shutdown.", IsActive = true },
                    new ProductionLogSetting { Id = 15, ProdStopType = "Tool Inspection / Preset", EfficiencyPerc = 100, Definition = "Tool presetting or inspection outside machine.", IsActive = true },

                    // ========== CATEGORY B : OPERATOR/PROCESS LOSS (0%) ==========
                    new ProductionLogSetting { Id = 16, ProdStopType = "Operator Shortage", EfficiencyPerc = 0, Definition = "Lack of operator availability resulting in stoppage.", IsActive = true },
                    new ProductionLogSetting { Id = 17, ProdStopType = "Rework", EfficiencyPerc = 0, Definition = "Machine stopped due to defects requiring reprocessing.", IsActive = true },
                    new ProductionLogSetting { Id = 18, ProdStopType = "Program Editing", EfficiencyPerc = 0, Definition = "Changes done to NC program during production.", IsActive = true },
                    new ProductionLogSetting { Id = 19, ProdStopType = "Store Delay", EfficiencyPerc = 0, Definition = "Delay in receiving materials or consumables.", IsActive = true },
                    new ProductionLogSetting { Id = 20, ProdStopType = "Program Search / Transfer", EfficiencyPerc = 0, Definition = "Waiting for NC program transfer or file retrieval.", IsActive = true },
                    new ProductionLogSetting { Id = 21, ProdStopType = "Part Truing / Inaccuracy (Grinding)", EfficiencyPerc = 0, Definition = "Grinding delay due to part correction or inaccuracy.", IsActive = true },
                    new ProductionLogSetting { Id = 22, ProdStopType = "Wrong Material Issued", EfficiencyPerc = 0, Definition = "Stoppage due to incorrect raw material issued.", IsActive = true },
                    new ProductionLogSetting { Id = 23, ProdStopType = "Material Quality Issue", EfficiencyPerc = 0, Definition = "Material found rejected before machining.", IsActive = true },
                    new ProductionLogSetting { Id = 24, ProdStopType = "In-Process Rejection", EfficiencyPerc = 0, Definition = "Scrap generated requiring stoppage.", IsActive = true },
                    new ProductionLogSetting { Id = 25, ProdStopType = "Drawing Not Available", EfficiencyPerc = 0, Definition = "Stoppage due to missing or unclear drawing.", IsActive = true },
                    new ProductionLogSetting { Id = 26, ProdStopType = "Revision Change Delay", EfficiencyPerc = 0, Definition = "Waiting for updated revision drawing or documents.", IsActive = true },
                    new ProductionLogSetting { Id = 27, ProdStopType = "Gauge / Instrument Unavailable", EfficiencyPerc = 0, Definition = "Stoppage due to missing inspection gauges.", IsActive = true },
                    new ProductionLogSetting { Id = 28, ProdStopType = "Operator Low Productivity", EfficiencyPerc = 0, Definition = "Delays due to slow operation or incorrect methods.", IsActive = true },
                    new ProductionLogSetting { Id = 29, ProdStopType = "Excess Idle Time", EfficiencyPerc = 0, Definition = "Operator not working despite machine availability.", IsActive = true },
                    new ProductionLogSetting { Id = 30, ProdStopType = "Document Missing / Route Card Missing", EfficiencyPerc = 0, Definition = "Stoppage due to missing production documents.", IsActive = true }
                );
            }
        }

}
