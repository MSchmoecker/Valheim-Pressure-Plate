using System;
using UnityEngine;

namespace PressurePlate.Compatibility.WardIsLove {
    public static class WardMonoscriptExt {
        public static Type ClassType() {
            return Type.GetType("WardIsLove.Extensions.WardMonoscriptExt, WardIsLove");
        }

        public static WardMonoscript GetWardMonoscript(Vector3 pos) {
            object script = ModCompat.InvokeMethod<object>(ClassType(), null, "GetWardMonoscript", new object[] { pos });
            return new WardMonoscript(script);
        }

        public static bool GetDoorInteractOn(this WardMonoscript wrapper) {
            return ModCompat.InvokeMethod<bool>(ClassType(), null, "GetDoorInteractOn", new[] { wrapper.targetScript });
        }
    }
}
