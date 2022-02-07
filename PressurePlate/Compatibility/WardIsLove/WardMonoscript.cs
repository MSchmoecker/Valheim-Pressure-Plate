using System;
using UnityEngine;

namespace PressurePlate.Compatibility.WardIsLove {
    public class WardMonoscript {
        public object targetScript;

        public static Type ClassType() {
            return Type.GetType("WardIsLove.Util.WardMonoscript, WardIsLove");
        }

        public WardMonoscript(object targetScript) {
            this.targetScript = targetScript;
        }

        public static bool CheckInWardMonoscript(Vector3 point, bool flash = false) {
            return ModCompat.InvokeMethod<bool>(ClassType(), null, "CheckInWardMonoscript", new object[] { point, flash });
        }

        public ZNetView GetZNetView() {
            if (targetScript == null) {
                return null;
            }

            return ModCompat.GetField<ZNetView>(ClassType(), targetScript, "m_nview");
        }

        public ZDO GetZDO() {
            ZNetView netView = GetZNetView();
            return netView != null && netView && netView.IsValid() ? netView.GetZDO() : null;
        }
    }
}
