/**
    ReSharper disable once InvalidXmlDocComment

    Copyright 2022 [information redacted]

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using ABI_RC.Core.UI;
using cohtml;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;

namespace NotificationAPI
{
    // A lot of ReSharper's recommendations are ignored intentionally in order to expose
    // MelonPreferences, and the Data & Relative paths to calling assemblies.
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class NotificationAPI : MelonMod
    {
        #region MelonPreference definitions
        private static MelonPreferences_Category _preferencesCategory;
        private static MelonPreferences_Entry<bool> _patchViewDropsEnabled;
        
        internal static MelonPreferences_Entry<bool> AutomaticEngineUpgradesEnabled;

        public static MelonPreferences_Entry<bool> CustomThemePositionEnabled;
        public static MelonPreferences_Entry<string> CustomThemePositionUnit;
        public static MelonPreferences_Entry<int> CustomThemePositionTop;
        public static MelonPreferences_Entry<int> CustomThemePositionBottom;
        public static MelonPreferences_Entry<int> CustomThemePositionLeft;
        public static MelonPreferences_Entry<int> CustomThemePositionRight;
        public static MelonPreferences_Entry<int> CustomThemePositionHeight;
        public static MelonPreferences_Entry<int> CustomThemePositionWidth;
        public static MelonPreferences_Entry<string> DefaultNotificationTheme;
        #endregion
        
        #region NotificationAPI internal variables
        private static MelonLogger.Instance _logger;
        private static CohtmlView _hudView;
        private static bool _occupied;
        private static readonly Queue<Notification> Queue = new Queue<Notification>();

        internal static HarmonyLib.Harmony HarmonyInst;
        internal static Assembly CurrentAssembly;
        internal static string Javascript;
        #endregion

        #region Publicly-exposed variables for callers
        public static readonly string DataPath = "ChilloutVR_Data\\StreamingAssets\\Cohtml\\UIResources\\NotificationAPI\\";
        public static string RelativePath;
        #endregion
        
        #region NotificationAPI internal hooks, coroutines & callbacks
        public override void OnApplicationStart()
        {
            #region Initial variable definitions
            HarmonyInst = new HarmonyLib.Harmony("NotificationAPI");
            _preferencesCategory = MelonPreferences.CreateCategory("NotificationAPI");
            _patchViewDropsEnabled = _preferencesCategory.CreateEntry("PatchViewDropsEnabled", false, "Patch in-game notification systems");
            AutomaticEngineUpgradesEnabled = _preferencesCategory.CreateEntry("AutomaticEngineUpgradesEnabled", true,
                "Allow the automatic upgrading of the notification engine JS. Disable if you have a custom fork.");
            DefaultNotificationTheme = _preferencesCategory.CreateEntry("DefaultNotificationTheme", "payday",
                "The theme that will be used for notifications that do not specify a custom one.");

            CustomThemePositionEnabled = _preferencesCategory.CreateEntry("CustomThemePositionEnabled", false, "Whether the custom placement of the notification element is enabled.");
            CustomThemePositionUnit = _preferencesCategory.CreateEntry("CustomThemePositionUnit", "percent", "What unit are the Top/Bottom/Left/Right measurements in? (pixels/percent)");
            CustomThemePositionTop = _preferencesCategory.CreateEntry("CustomThemePositionTop", 0, "What is the HUD offset (from the top) that the notification element should appear at?");
            CustomThemePositionBottom = _preferencesCategory.CreateEntry("CustomThemePositionBottom", 0, "What is the HUD offset (from the bottom) that the notification element should appear at?");
            CustomThemePositionLeft = _preferencesCategory.CreateEntry("CustomThemePositionLeft", 0, "What is the HUD offset (from the left) that the notification element should appear at?");
            CustomThemePositionRight = _preferencesCategory.CreateEntry("CustomThemePositionRight", 0, "What is the HUD offset (from the right) that the notification element should appear at?");
            CustomThemePositionHeight = _preferencesCategory.CreateEntry("CustomThemePositionHeight", 0, "The height of the notification element in pixels.");
            CustomThemePositionWidth = _preferencesCategory.CreateEntry("CustomThemePositionWidth", 0, "The width of the notification element in pixels.");

            #region Preference event listeners
            _patchViewDropsEnabled.OnValueChanged += PreferenceHooks.HarmonyPatchOptionUpdated;
            CustomThemePositionEnabled.OnValueChanged += PreferenceHooks.CustomPositionSettingUpdatedBool;
            CustomThemePositionUnit.OnValueChanged += PreferenceHooks.CustomPositionSettingUpdatedString;
            CustomThemePositionTop.OnValueChanged += PreferenceHooks.CustomPositionSettingUpdatedInt;
            CustomThemePositionBottom.OnValueChanged += PreferenceHooks.CustomPositionSettingUpdatedInt;
            CustomThemePositionLeft.OnValueChanged += PreferenceHooks.CustomPositionSettingUpdatedInt;
            CustomThemePositionRight.OnValueChanged += PreferenceHooks.CustomPositionSettingUpdatedInt;
            CustomThemePositionHeight.OnValueChanged += PreferenceHooks.CustomPositionSettingUpdatedInt;
            CustomThemePositionWidth.OnValueChanged += PreferenceHooks.CustomPositionSettingUpdatedInt;
            #endregion

            if (_patchViewDropsEnabled.Value)
            {
                HarmonyInst.PatchAll(typeof(Patches));
            }

            _logger = LoggerInstance;
            CurrentAssembly = Assembly.GetExecutingAssembly();
            #endregion
            
            MelonCoroutines.Start(OnUiManagerInit());

            #region Initial setup & engine/theme upgrades
            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
            if (!Directory.Exists($"{DataPath}\\themes")) Directory.CreateDirectory($"{DataPath}\\themes");
            if (!File.Exists($"{DataPath}\\notification_engine.js")) Util.WriteFile("notification_engine");
            if (AutomaticEngineUpgradesEnabled.Value) Util.WriteFile("notification_engine"); // TODO: Only write if changed
            Util.WriteDefaultThemes(DataPath);
            #endregion

            if (String.IsNullOrEmpty(Javascript)) Javascript = File.ReadAllText($"{DataPath}notification_engine.js");

            string cwd = Directory.GetCurrentDirectory();
            RelativePath = Util.RelativePath(
                $"{cwd}\\ChilloutVR_Data\\StreamingAssets\\Cohtml\\UIResources\\CVR_HUD\\default",
                $"{cwd}\\{DataPath}").Replace("\\", "/"); // Thanks, Windows. (Since we're in a `file://` URI, we need *NIX paths.
        }

        private IEnumerator NotificationRoutine()
        {
            while (true)
            {
                if (!_occupied && Queue.Count > 0)
                {
                    InternalNotify(Queue.Dequeue());
                }
                
                yield return new WaitForSeconds(0.5f);
            }
            // ReSharper disable once IteratorNeverReturns
        }
        
        
        private IEnumerator OnUiManagerInit()
        {
            while (CohtmlHud.Instance == null)
                yield return null;
            OnUiManagerInitCallback();
        }

        private void OnUiManagerInitCallback()
        {
            _hudView = (CohtmlView)typeof(CohtmlHud).GetField("hudView", BindingFlags.NonPublic | 
                                                                         BindingFlags.Instance)
                ?.GetValue(CohtmlHud.Instance);

            if (_hudView == null)
            {
                _logger.Error("Could not retrieve hudView, exiting.");
                return;
            }
            
            _hudView.View.AddInitialScript(Javascript);
            _hudView.Listener.ReadyForBindings += () =>
            {
                _hudView.View.BindCall("notificationDoneCallback", new Action(CallbackFree));
                _hudView.View.BindCall("notificationThemeInitCallback", new Action(CallbackThemeInit));
            };
            
            MelonCoroutines.Start(NotificationRoutine());
        }
        
        /// <summary>
        /// Sends a Notification object to the JavaScript engine.
        /// </summary>
        /// <param name="notification">The Notification object to send in.</param>
        private static void InternalNotify(Notification notification)
        {
            _occupied = true;
            _hudView.View.TriggerEvent("notificationApi_Notify",
                notification.Msg,
                notification.Iterations,
                notification.Duration,
                notification.Color,
                notification.Theme);
        }

        /// <summary>
        /// Callback from the JavaScript engine to let the notification coroutine know it's allowed to send a new one.
        /// </summary>
        private void CallbackFree()
        {
            _occupied = false;
        }

        /// <summary>
        /// Callback from the JavaScript engine to allow for the registration of user preferences, paths, and themes.
        /// </summary>
        private void CallbackThemeInit()
        {
            _hudView.View.TriggerEvent("__notificationApiInternal_setUserDataPath", RelativePath);
            if (CustomThemePositionEnabled.Value) CustomPositionPreferencesUpdated();
            RegisterThemes();
        }

        /// <summary>
        /// Iterates through available themes and registers them.
        /// </summary>
        private void RegisterThemes()
        {
            foreach (var dir in Directory.EnumerateDirectories($"{DataPath}themes\\"))
            {
                var dirName = new DirectoryInfo(dir).Name;
                _logger.Msg($"Loading theme: \"{dirName}\"");
                RegisterTheme(dirName);
            }
        }
        
        /// <summary>
        /// Callback for when the PreferenceHook for a custom position setting is triggered.
        /// </summary>
        internal static void CustomPositionPreferencesUpdated()
        {
            var pref = new CustomPositionPreferences(CustomThemePositionEnabled.Value,
                CustomThemePositionUnit.Value,
                CustomThemePositionTop.Value,
                CustomThemePositionBottom.Value,
                CustomThemePositionLeft.Value,
                CustomThemePositionRight.Value,
                CustomThemePositionHeight.Value,
                CustomThemePositionWidth.Value);

            _hudView.View.TriggerEvent("__notificationApiInternal_CustomPositionPreferencesUpdated", JsonConvert.SerializeObject(pref));
        }
        #endregion
        
        #region Exposed APIs
        #region Obsolete APIs
        /// <summary>
        /// Creates a default Notification struct with the provided message and queues it.
        /// </summary>
        /// <param name="msg">The message that the notification should display.</param>
        [Obsolete("Notify(string msg) is deprecated, please use Notify(Notification notification) instead.")]
        public static void Notify(string msg)
        {
            Queue.Enqueue(new Notification(msg));
        }
        
        /// <summary>
        /// Creates a custom Notification struct with the provided message and queues it.
        /// </summary>
        /// <param name="msg">The message that the notification should display.</param>
        /// <param name="animationIterations">How many times the notification's animation should play.</param>
        [Obsolete("Notify(string msg, int animationIterations) is deprecated, please use Notify(Notification notification) instead.")]
        public static void Notify(string msg, int animationIterations)
        {
            Queue.Enqueue(new Notification(msg, animationIterations));
        }
        
        /// <summary>
        /// Creates a custom Notification struct with the provided message and queues it.
        /// </summary>
        /// <param name="msg">The message that the notification should display.</param>
        /// <param name="animationIterations">How many times the notification's animation should play.</param>
        /// <param name="animationDuration">How many seconds the animation should take to finish.</param>
        [Obsolete("Notify(string msg, int animationIterations, int animationDuration) is deprecated, please use Notify(Notification notification) instead.")]
        public static void Notify(string msg, int animationIterations, int animationDuration)
        {
            Queue.Enqueue(new Notification(msg, animationIterations, animationDuration));
        }
        #endregion

        /// <summary>
        /// Takes a Notification struct and queues it.
        /// </summary>
        /// <param name="notification">The Notification struct to queue.</param>
        public static void Notify(Notification notification)
        {
            Queue.Enqueue(notification);
        }
        
                /// <summary>
        /// Registers a theme. At this time, there should be no need to manually register themes,
        /// as they are all discovered by default in RegisterThemes(), but this may be expanded to allow for custom
        /// theme paths in the future.
        /// </summary>
        /// <param name="themeName">The name of the theme to register.</param>
        public static void RegisterTheme(string themeName)
        {
            string themePath = $"{DataPath}themes\\{themeName}\\";
            string themeContent = "";
            string themeNotifyCallback = "";
            string themeEndNotificationCallback = "";
            string stylesheetFile = "";

            if (File.Exists($"{themePath}content.html"))
            {
                themeContent = File.ReadAllText($"{themePath}content.html");
                if (string.IsNullOrEmpty(themeContent))
                {
                    _logger.Warning($"Theme \"{themeName}\" did not have content, not loading it.");
                    return;
                }
            }

            if (File.Exists($"{themePath}notification_start.js"))
                themeNotifyCallback = File.ReadAllText($"{themePath}notification_start.js");

            if (File.Exists($"{themePath}notification_end.js"))
                themeEndNotificationCallback = File.ReadAllText($"{themePath}notification_end.js");

            if (File.Exists($"{themePath}notification.css"))
                stylesheetFile = File.ReadAllText($"{themePath}notification.css");

            if (string.IsNullOrEmpty(themeEndNotificationCallback))
                themeEndNotificationCallback = "engine.call(\"notificationDoneCallback\");";

            if (!string.IsNullOrEmpty(themeEndNotificationCallback) &&
                !themeEndNotificationCallback.Contains("// NotificationAPIAnnotation: DO_NOT_APPEND_ENGINE_CALLBACK"))
                themeEndNotificationCallback += "\n\nengine.call(\"notificationDoneCallback\");";


            _hudView.View.TriggerEvent("notificationApi_LoadTheme", themeName, 
                themeContent,
                stylesheetFile,
                themeNotifyCallback, 
                themeEndNotificationCallback);
        }
        #endregion
    }
}

public struct Notification
{
    public readonly string Msg;
    public readonly int Iterations;
    public readonly int Duration;
    public readonly string Color; // NOTE: Whether this setting applies is dependent on whether the current theme supports custom colors.
    public readonly string Theme;

    public Notification(string msg, int iterations = 3, int duration = 8, string color = "#ffef00", string theme = "")
    {
        if (theme == "") theme = NotificationAPI.NotificationAPI.DefaultNotificationTheme.Value;
        Msg = msg;
        Iterations = iterations;
        Duration = duration;
        Color = color;
        Theme = theme;
    }
}


internal struct CustomPositionPreferences
{
    [JsonProperty("enabled")] public readonly bool Enabled;
    [JsonProperty("type")] public readonly string Type;
    [JsonProperty("top")] public readonly int Top;
    [JsonProperty("bottom")] public readonly int Bottom;
    [JsonProperty("left")] public readonly int Left;
    [JsonProperty("right")] public readonly int Right;
    [JsonProperty("height")] public readonly int Height;
    [JsonProperty("width")] public readonly int Width;

    internal CustomPositionPreferences(bool enabled, string type, int top, int bottom, int left, int right, int height, int width)
    {
        Enabled = enabled;
        Type = type;
        Top = top;
        Bottom = bottom;
        Left = left;
        Right = right;
        Height = height;
        Width = width;
    }
}