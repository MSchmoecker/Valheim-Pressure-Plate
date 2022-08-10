using System;
using BepInEx.Bootstrap;
using BepInEx.Configuration;

namespace PressurePlate.Compatibility.WardIsLove {
    public class WardIsLovePlugin : ModCompat {
        private const string GUID = "azumatt.WardIsLove";
        private static readonly System.Version MinVersion = new System.Version(2, 3, 3);

        public static Type ClassType() {
            return Type.GetType("WardIsLove.WardIsLovePlugin, WardIsLove");
        }

        public static bool IsLoaded() {
            return Chainloader.PluginInfos.ContainsKey(GUID) && Chainloader.PluginInfos[GUID].Metadata.Version >= MinVersion;
        }

        public static ConfigEntry<bool> WardEnabled() {
            return GetField<ConfigEntry<bool>>(ClassType(), null, "WardEnabled");
        }
    }
}
