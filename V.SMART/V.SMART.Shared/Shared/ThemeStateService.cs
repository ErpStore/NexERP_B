using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Shared
{
    public class ThemeStateService
    {
        public bool IsDarkMode { get; private set; }

        public event Action? OnChange;

        public void SetTheme(bool isDark)
        {
            IsDarkMode = isDark;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}
