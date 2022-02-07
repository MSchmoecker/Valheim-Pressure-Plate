using UnityEngine;

namespace PressurePlate.Compatibility.WardIsLove {
    public static class DoorInteract {
        public static bool InsideWard(Vector3 pos) {
            return WardIsLovePlugin.WardEnabled().Value && WardMonoscript.CheckInWardMonoscript(pos);
        }

        public static bool CanInteract(Player player) {
            Vector3 pos = player.transform.position;
            return CustomCheck.CheckAccess(player.GetPlayerID(), pos, 0f, false) || WardMonoscriptExt.GetWardMonoscript(pos).GetDoorInteractOn();
        }
    }
}
