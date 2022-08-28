using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ABI_RC.Core.UI;
using cohtml;
using MelonLoader;
using UnityEngine;

namespace NotificationAPI
{
    public class NotificationAPI : MelonMod
    {
        private static MelonPreferences_Category _preferencesCategory;
        private static MelonPreferences_Entry<bool> _patchViewDropsEnabled;
        public static MelonPreferences_Entry<string> DefaultNotificationTheme;
        private static HarmonyLib.Harmony _harmony;
        private static MelonLogger.Instance _logger;
        private static CohtmlView _hudView;
        private static bool _occupied;
        
        private static readonly Queue<Notification> Queue = new Queue<Notification>();
        public static readonly string DataPath = "ChilloutVR_Data\\StreamingAssets\\Cohtml\\UIResources\\NotificationAPI\\";
        public static string RelativePath;
        internal static Assembly CurrentAssembly;
        internal static string Javascript;

        public override void OnApplicationStart()
        {
            _harmony = new HarmonyLib.Harmony("NotificationAPI");
            _preferencesCategory = MelonPreferences.CreateCategory("NotificationAPI");
            _patchViewDropsEnabled = _preferencesCategory.CreateEntry("PatchViewDropsEnabled", false, "Patch in-game notification systems");
            DefaultNotificationTheme = _preferencesCategory.CreateEntry("DefaultNotificationTheme", "payday",
                "The theme that will be used for notifications that do not specify a custom one.");
            _patchViewDropsEnabled.OnValueChanged += HarmonyPatchOptionUpdated;
            if (_patchViewDropsEnabled.Value)
            {
                _harmony.PatchAll(typeof(Patches));
            }

            _logger = LoggerInstance;
            CurrentAssembly = Assembly.GetExecutingAssembly();
            
            MelonCoroutines.Start(OnUiManagerInit());

            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
            if (!Directory.Exists($"{DataPath}\\themes")) Directory.CreateDirectory($"{DataPath}\\themes");
            if (!File.Exists($"{DataPath}\\notification_engine.js")) Util.WriteFile("notification_engine");
            Util.WriteDefaultThemes(DataPath);

            if (String.IsNullOrEmpty(Javascript)) Javascript = File.ReadAllText($"{DataPath}notification_engine.js");

            string cwd = Directory.GetCurrentDirectory();
            RelativePath = Util.RelativePath(
                $"{cwd}\\ChilloutVR_Data\\StreamingAssets\\Cohtml\\UIResources\\CVR_HUD\\default",
                $"{cwd}\\{DataPath}").Replace("\\", "/");
        }

        private void HarmonyPatchOptionUpdated(bool b1, bool b2)
        {
            if (b1 || b2)
            {
                _harmony.PatchAll(typeof(Patches));
            }
            else
            {
                _harmony.UnpatchSelf();
            }
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

        public static void Notify(string msg)
        {
            Queue.Enqueue(new Notification(msg));
        }
        
        public static void Notify(string msg, int animationIterations)
        {
            Queue.Enqueue(new Notification(msg, animationIterations));
        }
        
        public static void Notify(string msg, int animationIterations, int animationDuration)
        {
            Queue.Enqueue(new Notification(msg, animationIterations, animationDuration));
        }

        public static void Notify(Notification notification)
        {
            Queue.Enqueue(notification);
        }

        private void RegisterThemes()
        {
            foreach (var dir in Directory.EnumerateDirectories($"{DataPath}themes\\"))
            {
                var _dir = new DirectoryInfo(dir).Name;
                _logger.Msg($"Loading theme: \"{_dir}\"");
                RegisterTheme(_dir);
            }
        }
        
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

            stylesheetFile = "notification.css";

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

        private static void InternalNotify(Notification notification)
        {
            _occupied = true;
            _hudView.View.TriggerEvent("notificationApi_Notify", notification.Msg, notification.Iterations, notification.Duration);
        }

        private void CallbackFree()
        {
            _occupied = false;
        }

        private void CallbackThemeInit()
        {
            _hudView.View.TriggerEvent("__notificationApiInternal_setUserDataPath", RelativePath);
            RegisterThemes();
        }
    }
}

public struct Notification
{
    public readonly string Msg;
    public readonly int Iterations;
    public readonly int Duration;
    public readonly string Color;
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