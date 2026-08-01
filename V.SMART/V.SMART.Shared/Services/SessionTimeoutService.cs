using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    public class SessionTimeoutService
    {
        private DateTime _lastActivity = DateTime.Now;

        public void Touch()
        {
            _lastActivity = DateTime.Now;
        }

        public bool IsIdle(int minutes)
        {
            return DateTime.Now.Subtract(_lastActivity)
                               .TotalMinutes >= minutes;
        }
    }
}
