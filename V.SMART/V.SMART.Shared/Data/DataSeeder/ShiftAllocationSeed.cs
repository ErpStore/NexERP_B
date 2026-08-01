using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.DataSeeder
{
    public static class ShiftAllocationSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShiftAllocation>().HasData(
                new ShiftAllocation
                {
                    ShiftId = 1,
                    ShiftCode = "A",
                    ShiftName = "Morning Shift",
                    ShiftStartTime = new TimeOnly(6, 0),
                    ShiftEndTime = new TimeOnly(14, 0),
                    BreakStartTime = new TimeOnly(10, 0),
                    BreakEndTime = new TimeOnly(10, 30),
                    TotalWorkingHours = new TimeSpan(8, 0, 0),
                    OverTimeAllowed = false,
                    LateMarkAllowed = 10,
                    EarlyLeaveAllowed = 10,
                    CreatedBy = "System",
                    CreatedDate = new DateTime(2024, 01, 01)
                },
                new ShiftAllocation
                {
                    ShiftId = 2,
                    ShiftCode = "B",
                    ShiftName = "Evening Shift",
                    ShiftStartTime = new TimeOnly(14, 0),
                    ShiftEndTime = new TimeOnly(22, 0),
                    BreakStartTime = new TimeOnly(18, 0),
                    BreakEndTime = new TimeOnly(18, 30),
                    TotalWorkingHours = new TimeSpan(8, 0, 0),
                    OverTimeAllowed = false,
                    LateMarkAllowed = 10,
                    EarlyLeaveAllowed = 10,
                    CreatedBy = "System",
                    CreatedDate = new DateTime(2024, 01, 01)
                },
                new ShiftAllocation
                {
                    ShiftId = 3,
                    ShiftCode = "C",
                    ShiftName = "Night Shift",
                    ShiftStartTime = new TimeOnly(22, 0),
                    ShiftEndTime = new TimeOnly(6, 0),
                    BreakStartTime = new TimeOnly(2, 0),
                    BreakEndTime = new TimeOnly(2, 30),
                    TotalWorkingHours = new TimeSpan(8, 0, 0),
                    OverTimeAllowed = false,
                    LateMarkAllowed = 10,
                    EarlyLeaveAllowed = 10,
                    CreatedBy = "System",
                    CreatedDate = new DateTime(2024, 01, 01)
                },
                new ShiftAllocation
                {
                    ShiftId = 4,
                    ShiftCode = "D",
                    ShiftName = "Flex Day",
                    ShiftStartTime = new TimeOnly(8, 0),
                    ShiftEndTime = new TimeOnly(16, 0),
                    BreakStartTime = new TimeOnly(12, 0),
                    BreakEndTime = new TimeOnly(12, 30),
                    TotalWorkingHours = new TimeSpan(8, 0, 0),
                    OverTimeAllowed = false,
                    LateMarkAllowed = 10,
                    EarlyLeaveAllowed = 10,
                    CreatedBy = "System",
                    CreatedDate = new DateTime(2024, 01, 01)
                },
                new ShiftAllocation
                {
                    ShiftId = 5,
                    ShiftCode = "G",
                    ShiftName = "Graveyard Shift",
                    ShiftStartTime = new TimeOnly(0, 0),
                    ShiftEndTime = new TimeOnly(8, 0),
                    BreakStartTime = new TimeOnly(4, 0),
                    BreakEndTime = new TimeOnly(4, 30),
                    TotalWorkingHours = new TimeSpan(8, 0, 0),
                    OverTimeAllowed = false,
                    LateMarkAllowed = 10,
                    EarlyLeaveAllowed = 10,
                    CreatedBy = "System",
                    CreatedDate = new DateTime(2024, 01, 01)
                },
                new ShiftAllocation
                {
                    ShiftId = 6,
                    ShiftCode = "E",
                    ShiftName = "Executive Shift",
                    ShiftStartTime = new TimeOnly(9, 0),
                    ShiftEndTime = new TimeOnly(17, 0),
                    BreakStartTime = new TimeOnly(13, 0),
                    BreakEndTime = new TimeOnly(13, 30),
                    TotalWorkingHours = new TimeSpan(8, 0, 0),
                    OverTimeAllowed = false,
                    LateMarkAllowed = 10,
                    EarlyLeaveAllowed = 10,
                    CreatedBy = "System",
                    CreatedDate = new DateTime(2024, 01, 01)
                }
            );
        }
    }
}
