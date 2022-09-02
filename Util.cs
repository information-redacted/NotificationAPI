/**
    ReSharper disable once InvalidXmlDocComment

    Copyright 2022 [information redacted]

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.Text;

namespace NotificationAPI
{
    public class Util
    {

        internal static void WriteDefaultThemes(string dataPath)
        {
            string[] defaultThemes = new[] { "payday", "payday_info" };

            foreach (string theme in defaultThemes)
            {
                if (!Directory.Exists($"{dataPath}\\themes\\{theme}")) Directory.CreateDirectory($"{dataPath}\\themes\\{theme}");
            
                if (!File.Exists($"{dataPath}themes\\{theme}\\content.html")) WriteFile("theme_content", theme);
                if (!File.Exists($"{dataPath}themes\\{theme}\\notification_start.js")) WriteFile("theme_notification_start", theme);
                if (!File.Exists($"{dataPath}themes\\{theme}\\notification_end.js")) WriteFile("theme_notification_end", theme);
                if (!File.Exists($"{dataPath}themes\\{theme}\\notification.css")) WriteFile("theme_css", theme);
                if (!File.Exists($"{dataPath}themes\\{theme}\\notification.svg")) WriteFile("theme_indicator_svg", theme);
            }
        }
        
        internal static void WriteFile(string ftype, string themeName = "")
        {
            switch (ftype)
            {
                case "notification_engine":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.notification_engine.js"))
                        if (stream != null)
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                NotificationAPI.Javascript = reader.ReadToEnd();
                                var w = File.CreateText($"{NotificationAPI.DataPath}notification_engine.js");
                                w.Write(NotificationAPI.Javascript);
                                w.Flush();
                                w.Close();
                            }

                    break;
                }
                case "theme_content":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.themes.{themeName}.content.html"))
                        if (stream != null)
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var w = File.CreateText($"{NotificationAPI.DataPath}themes\\{themeName}\\content.html");
                                w.Write(reader.ReadToEnd());
                                w.Flush();
                                w.Close();
                            }

                    break;
                }
                case "theme_notification_start":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.themes.{themeName}.notification_start.js"))
                        if (stream != null)
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var w = File.CreateText($"{NotificationAPI.DataPath}themes\\{themeName}\\notification_start.js");
                                w.Write(reader.ReadToEnd());
                                w.Flush();
                                w.Close();
                            }

                    break;
                }
                case "theme_notification_end":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.themes.{themeName}.notification_end.js"))
                        if (stream != null)
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var w = File.CreateText($"{NotificationAPI.DataPath}themes\\{themeName}\\notification_end.js");
                                w.Write(reader.ReadToEnd());
                                w.Flush();
                                w.Close();
                            }

                    break;
                }
                case "theme_css":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.themes.{themeName}.notification.css"))
                        if (stream != null)
                            using (var reader = new StreamReader(stream))
                            {
                                var w = File.CreateText($"{NotificationAPI.DataPath}themes\\{themeName}\\notification.css");
                                w.Write(reader.ReadToEnd());
                                w.Flush();
                                w.Close();
                            }

                    break;
                }
                case "theme_indicator_svg":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.themes.{themeName}.notification.svg"))
                        if (stream != null)
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var w = File.CreateText($"{NotificationAPI.DataPath}themes\\{themeName}\\notification.svg");
                                w.Write(reader.ReadToEnd());
                                w.Flush();
                                w.Close();
                            }

                    break;
                }
            }
        }
        
        // This function has been directly taken from: https://mrpmorris.blogspot.com/2007/05/convert-absolute-path-to-relative-path.html
        internal static string RelativePath(string absPath, string relTo) {
            string[] absDirs = absPath.Split('\\');
            string[] relDirs = relTo.Split('\\');
  
            // Get the shortest of the two paths
            int len = absDirs.Length < relDirs.Length ? absDirs.Length : 
                relDirs.Length;

            // Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            // Find common root
            for (index = 0; index < len; index++) {
                if (absDirs[index] == relDirs[index]) lastCommonRoot = index;
                else break;
            }

            // If we didn't find a common prefix then throw
            if (lastCommonRoot == -1) {
                throw new ArgumentException("Paths do not have a common base");
            }

            // Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            // Add on the ..
            for (index = lastCommonRoot + 1; index < absDirs.Length; index++) {
                if (absDirs[index].Length > 0) relativePath.Append("..\\");
            }
  
            // Add on the folders
            for (index = lastCommonRoot + 1; index < relDirs.Length - 1; index++) {
                relativePath.Append(relDirs[index] + "\\");
            }
            relativePath.Append(relDirs[relDirs.Length - 1]);
  
            return relativePath.ToString();
        }
    }
}