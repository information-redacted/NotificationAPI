using HarmonyLib;

namespace NotificationAPI
{
    [HarmonyPatch(typeof(ABI_RC.Core.UI.CohtmlHud))]
    public class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch("ViewDropText", new []{ typeof(string), typeof(string)} )]
        private static bool ViewDropTextTwo(string headline, string small)
        {
            NotificationAPI.Notify($"{headline}  -  {small}", 1);
            return false;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch("ViewDropText", new []{ typeof(string), typeof(string), typeof(string)} )]
        [HarmonyPatch("ViewDropTextImmediate")]
        [HarmonyPatch("ViewDropTextLonger")]
        [HarmonyPatch("ViewDropTextLong")]
        private static bool ViewDropTextThree(string cat, string headline, string small)
        {
            NotificationAPI.Notify($"{cat}  -  {headline}  -  {small}", 1);
            return false;
        }
    }
}