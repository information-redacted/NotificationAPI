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
        private static bool _occupied;
        private static readonly Queue<Notification> Queue = new Queue<Notification>();
        private static MelonPreferences_Category _preferencesCategory;
        private static MelonPreferences_Entry<bool> _patchViewDropsEnabled;
        private static HarmonyLib.Harmony _harmony;
        private static MelonLogger.Instance _logger;
        private static CohtmlView _hudView;

        public static readonly string DataPath = "ChilloutVR_Data\\StreamingAssets\\Cohtml\\UIResources\\NotificationAPI\\";
        internal static Assembly CurrentAssembly;
        internal static string Javascript;
        internal static string ElementHtml;

        public override void OnApplicationStart()
        {
            _harmony = new HarmonyLib.Harmony("NotificationAPI");
            _preferencesCategory = MelonPreferences.CreateCategory("NotificationAPI");
            _patchViewDropsEnabled = _preferencesCategory.CreateEntry("PatchViewDropsEnabled", false, "Patch in-game notification systems");
            _patchViewDropsEnabled.OnValueChanged += HarmonyPatchOptionUpdated;
            if (_patchViewDropsEnabled.Value)
            {
                _harmony.PatchAll(typeof(Patches));
            }

            _logger = LoggerInstance;
            CurrentAssembly = Assembly.GetExecutingAssembly();
            MelonCoroutines.Start(OnUiManagerInit());

            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
            if (!File.Exists($"{DataPath}notification.js")) Util.WriteFile("js");
            if (!File.Exists($"{DataPath}notification.html")) Util.WriteFile("html");
            if (!File.Exists($"{DataPath}notification.css")) Util.WriteFile("css");
            if (!File.Exists($"{DataPath}notification.svg")) Util.WriteFile("svg");
            
            if (String.IsNullOrEmpty(Javascript)) Javascript = File.ReadAllText($"{DataPath}notification.js");
            if (String.IsNullOrEmpty(ElementHtml)) ElementHtml = File.ReadAllText($"{DataPath}notification.html");

            string cwd = Directory.GetCurrentDirectory();
            string relPath = Util.RelativePath(
                $"{cwd}\\ChilloutVR_Data\\StreamingAssets\\Cohtml\\UIResources\\CVR_HUD\\default",
                $"{cwd}\\{DataPath}").Replace("\\", "/");
            
            ElementHtml = ElementHtml.Replace("{{userDataPath}}", relPath);
            
            Javascript = Javascript.Replace("{{htmlTemplate}}", ElementHtml);
            Javascript = Javascript.Replace("{{userDataPath}}", relPath);
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
                    var n = Queue.Dequeue();
                    InternalNotify(n.Msg, n.Iterations, n.Duration);
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

        private static void InternalNotify(string msg, int animationIterations, int animationDuration)
        {
            _occupied = true;
            _hudView.View.TriggerEvent("notificationApi_Notify", msg, animationIterations, animationDuration);
        }

        private void CallbackFree()
        {
            _occupied = false;
        }
    }
}

struct Notification
{
    public readonly string Msg;
    public readonly int Iterations;
    public readonly int Duration;

    public Notification(string msg, int iterations = 3, int duration = 8)
    {
        Msg = msg;
        Iterations = iterations;
        Duration = duration;
    }
}