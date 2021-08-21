namespace PressurePlate {
    public class PlateData {
        public float triggerRadiusHorizontal;
        public float triggerRadiusVertical;
        public float openRadiusHorizontal;
        public float openRadiusVertical;
        public float openTime;
        public float triggerDelay;
        public bool invert;
        public bool ignoreWards;

        public void GetData(Plate plate) {
            triggerRadiusHorizontal = plate.GetTriggerRadiusHorizontal();
            triggerRadiusVertical = plate.GetTriggerRadiusVertical();
            openRadiusHorizontal = plate.GetOpenRadiusHorizontal();
            openRadiusVertical = plate.GetOpenRadiusVertical();
            openTime = plate.GetOpenTime();
            triggerDelay = plate.GetTriggerDelay();
            invert = plate.GetInvert();
            ignoreWards = plate.GetIgnoreWards();
        }

        public void SetData(Plate plate) {
            ZDO zdo = plate.zNetView.GetZDO();
            zdo.Set(Plate.KeyTriggerRadiusHorizontal, triggerRadiusHorizontal);
            zdo.Set(Plate.KeyTriggerRadiusVertical, triggerRadiusVertical);
            zdo.Set(Plate.KeyOpenRadiusHorizontal, openRadiusHorizontal);
            zdo.Set(Plate.KeyOpenRadiusVertical, openRadiusVertical);
            zdo.Set(Plate.KeyOpenTime, openTime);
            zdo.Set(Plate.KeyTriggerDelay, triggerDelay);
            zdo.Set(Plate.KeyInvert, invert);
            zdo.Set(Plate.KeyIgnoreWards, ignoreWards);
        }
    }
}
