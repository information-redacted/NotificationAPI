/**
    ReSharper disable once InvalidXmlDocComment

    Copyright 2022 [information redacted]

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

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