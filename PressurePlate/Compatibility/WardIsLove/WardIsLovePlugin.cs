using System;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine;
//using WardIsLove.Util;

namespace PressurePlate.Compatibility.WardIsLove {
    public class WardIsLovePlugin {
        public static Type ClassType() {
            return Type.GetType("WardIsLove.WardIsLovePlugin, WardIsLove");
        }

        public static bool IsLoaded() {
            return Chainloader.PluginInfos.ContainsKey("azumatt.WardIsLove");
        }

        public static ConfigEntry<bool> WardEnabled() {
            return ModCompat.GetField<ConfigEntry<bool>>(ClassType(), null, "_wardEnabled");
        }
    }
}
