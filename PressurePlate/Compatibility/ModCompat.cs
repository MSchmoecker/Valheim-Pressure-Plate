using System;
using BepInEx.Bootstrap;

namespace PressurePlate.Compatibility {
    public class ModCompat {
        public static T InvokeMethod<T>(Type type, object instance, string methodName, object[] parameter) {
            return (T)type.GetMethod(methodName)?.Invoke(instance, parameter);
        }

        public static T GetField<T>(Type type, object instance, string fieldName) {
            return (T)type.GetField(fieldName)?.GetValue(instance);
        }
    }
}
