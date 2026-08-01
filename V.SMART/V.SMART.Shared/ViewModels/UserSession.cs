using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels
{
    public class UserSession
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string DatabaseName { get; set; }
        public string HostName { get; set; }
    }
}
