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
            plate.SetTriggerRadiusHorizontal(triggerRadiusHorizontal);
            plate.SetTriggerRadiusVertical(triggerRadiusVertical);
            plate.SetOpenRadiusHorizontal(openRadiusHorizontal);
            plate.SetOpenRadiusVertical(openRadiusVertical);
            plate.SetOpenTime(openTime);
            plate.SetTriggerDelay(triggerDelay);
            plate.SetInvert(invert);
            plate.SetIgnoreWards(ignoreWards);
        }
    }
}
