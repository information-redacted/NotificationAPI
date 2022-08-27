using System;
using System.IO;
using System.Text;

namespace NotificationAPI
{
    public class Util
    {
        internal static void WriteFile(string ftype)
        {
            switch (ftype)
            {
                case "js":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.notification.{ftype}"))
                        if (stream != null)
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                NotificationAPI.Javascript = reader.ReadToEnd();
                                var w = File.CreateText($"{NotificationAPI.DataPath}notification.{ftype}");
                                w.Write(NotificationAPI.Javascript);
                                w.Flush();
                                w.Close();
                            }

                    break;
                }
                case "html":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.notification.{ftype}"))
                        if (stream != null)
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                NotificationAPI.ElementHtml = reader.ReadToEnd();
                                var w = File.CreateText($"{NotificationAPI.DataPath}notification.{ftype}");
                                w.Write(NotificationAPI.ElementHtml);
                                w.Flush();
                                w.Close();
                            }

                    break;
                }
                case "css":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.notification.{ftype}"))
                        if (stream != null)
                            using (var reader = new StreamReader(stream))
                            {
                                var w = File.CreateText($"{NotificationAPI.DataPath}notification.{ftype}");
                                w.Write(reader.ReadToEnd());
                                w.Flush();
                                w.Close();
                            }

                    break;
                }
                case "svg":
                {
                    using (Stream stream = NotificationAPI.CurrentAssembly.GetManifestResourceStream($"NotificationAPI.notification.{ftype}"))
                        if (stream != null)
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var w = File.CreateText($"{NotificationAPI.DataPath}notification.{ftype}");
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