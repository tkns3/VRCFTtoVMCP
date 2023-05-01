using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace VRCFTtoVMCP
{
    internal static class VRChat
    {
        public static readonly string VRCDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "VRChat", "VRChat");
        public static readonly string VRCOSCFolder = Path.Combine(VRCDataFolder, "OSC");

        public static void CreateVRCAvatarFile(string filename)
        {
            string jsonText = ResourceString(filename);
            string vrcAvatarsFolder = VRCAvatarsFolder();
            if (jsonText.Length > 0 && vrcAvatarsFolder.Length > 0)
            {
                using var writer = new StreamWriter(Path.Combine(vrcAvatarsFolder, filename), false, new UTF8Encoding(true));
                writer.Write(jsonText);
            }
        }

        public static string VRCAvatarsFolder()
        {
            foreach (var userFolder in Directory.GetDirectories(VRCOSCFolder))
            {
                string avatarsFolder = Path.Combine(userFolder, "Avatars");
                if (Directory.Exists(avatarsFolder))
                {
                    return avatarsFolder;
                }
            }
            return "";
        }

        public static string ResourceString(string filename)
        {
            string resource = $"VRCFTtoVMCP.Resources.{filename}";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream? stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using StreamReader reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            }
            return "";
        }
    }
}
