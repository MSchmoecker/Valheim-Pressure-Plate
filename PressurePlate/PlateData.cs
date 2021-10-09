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
        public bool allowMobs;

        public void GetData(Plate plate) {
            triggerRadiusHorizontal = plate.TriggerRadiusHorizontal;
            triggerRadiusVertical = plate.TriggerRadiusVertical;
            openRadiusHorizontal = plate.OpenRadiusHorizontal;
            openRadiusVertical = plate.OpenRadiusVertical;
            openTime = plate.OpenTime;
            triggerDelay = plate.TriggerDelay;
            invert = plate.Invert;
            ignoreWards = plate.IgnoreWards;
            allowMobs = plate.AllowMobs;
        }

        public void SetData(Plate plate) {
            plate.TriggerRadiusHorizontal = triggerRadiusHorizontal;
            plate.TriggerRadiusVertical = triggerRadiusVertical;
            plate.OpenRadiusHorizontal = openRadiusHorizontal;
            plate.OpenRadiusVertical = openRadiusVertical;
            plate.OpenTime = openTime;
            plate.TriggerDelay = triggerDelay;
            plate.Invert = invert;
            plate.IgnoreWards = ignoreWards;
            plate.AllowMobs = allowMobs;
        }
    }
}
