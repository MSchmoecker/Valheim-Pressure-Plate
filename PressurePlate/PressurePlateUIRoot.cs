using UnityEngine;

namespace PressurePlate {
    public class PressurePlateUIRoot : MonoBehaviour {
        private void Start() {
            gameObject.AddComponent<Localize>();
        }
    }
}
