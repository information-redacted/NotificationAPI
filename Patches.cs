/**
    ReSharper disable once InvalidXmlDocComment

    Copyright 2022 [information redacted]

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

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
            
            NotificationAPI.Notify(new Notification(msg, 1));
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
            
            NotificationAPI.Notify(new Notification(msg, 1));
            return false;
        }
    }
}