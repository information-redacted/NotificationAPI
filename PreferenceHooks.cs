namespace NotificationAPI
{
    public class PreferenceHooks
    {
        internal static void HarmonyPatchOptionUpdated(bool b1, bool b2)
        {
            if (b1 || b2)
            {
                NotificationAPI.HarmonyInst.PatchAll(typeof(Patches));
            }
            else
            {
                NotificationAPI.HarmonyInst.UnpatchSelf();
            }
        }
        
        internal static void CustomPositionSettingUpdatedBool(bool b1, bool b2) { NotificationAPI.CustomPositionPreferencesUpdated(); }
        internal static void CustomPositionSettingUpdatedInt(int i1, int i2) { NotificationAPI.CustomPositionPreferencesUpdated(); }
        internal static void CustomPositionSettingUpdatedString(string s1, string s2) { NotificationAPI.CustomPositionPreferencesUpdated(); }
    }
}