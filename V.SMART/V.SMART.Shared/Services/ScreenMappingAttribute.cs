using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScreenMappingAttribute : Attribute
    {
        public string ScreenName { get; }

        public ScreenMappingAttribute(string screenName)
        {
            ScreenName = screenName;
        }
    }
}
