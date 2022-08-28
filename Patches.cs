using HarmonyLib;

namespace NotificationAPI
{
    [HarmonyPatch(typeof(ABI_RC.Core.UI.CohtmlHud))]
    public class Patches
    {
        // TODO: While this mod is supposed to be a library for other mods to use,
        //       An implementation of native HUD notification overrides exists
        //       as a testing case. At this time, with the single theme we have,
        //       it doesn't make sense to extend them for custom Join/Leave cases.
        //       That possibility should be explored once theming and coloring is implemented.

        [HarmonyPrefix]
        [HarmonyPatch("ViewDropText", new []{ typeof(string), typeof(string)} )]
        private static bool ViewDropTextTwo(string headline, string small)
        {
            string msg = "";
            if (!string.IsNullOrEmpty(headline))
            {
                msg += headline;
                if (!string.IsNullOrEmpty(small)) msg += "  -  ";
            }

            if (!string.IsNullOrEmpty(small)) msg += small;
            
            NotificationAPI.Notify(msg, 1);
            return false;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch("ViewDropText", new []{ typeof(string), typeof(string), typeof(string)} )]
        [HarmonyPatch("ViewDropTextImmediate")]
        [HarmonyPatch("ViewDropTextLonger")]
        [HarmonyPatch("ViewDropTextLong")]
        private static bool ViewDropTextThree(string cat, string headline, string small)
        {
            string msg = "";
            if (!string.IsNullOrEmpty(cat))
            {
                msg += cat;
                if (!string.IsNullOrEmpty(headline)) msg += "  -  ";
            }

            if (!string.IsNullOrEmpty(headline))
            {
                msg += headline;
                if (!string.IsNullOrEmpty(small)) msg += "  -  ";
            }

            if (!string.IsNullOrEmpty(small)) msg += small;
            
            NotificationAPI.Notify(msg, 1);
            return false;
        }
    }
}