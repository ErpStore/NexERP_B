using V.SMART.Shared.Data.Master.Admin;

namespace V.SMART.Shared.Shared
{
    public static class RightsHelper
    {
        public static bool HasViewRight(List<UserRight> rights, string screenName) =>
            rights.FirstOrDefault(r => r.Screens.ScreenName == screenName)?.CanView ?? false;

        public static bool HasCreateRight(List<UserRight> rights, string screenName) =>
            rights.FirstOrDefault(r => r.Screens.ScreenName == screenName)?.CanCreate ?? false;

        public static bool HasEditRight(List<UserRight> rights, string screenName) =>
            rights.FirstOrDefault(r => r.Screens.ScreenName == screenName)?.CanEdit ?? false;

        public static bool HasDeleteRight(List<UserRight> rights, string screenName) =>
            rights.FirstOrDefault(r => r.Screens.ScreenName == screenName)?.CanDelete ?? false;

        public static bool IsHidden(List<UserRight> rights, string screenName) =>
            rights.FirstOrDefault(r => r.Screens.ScreenName == screenName)?.IsHide ?? false;
    }
}
