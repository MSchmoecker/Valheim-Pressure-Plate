using System.Reflection;

namespace PressurePlate {
    public static class PrivateHelper {
        public static T2 GetPrivateField<T1, T2>(T1 obj, string name) {
            FieldInfo field = typeof(T1).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T2) field.GetValue(obj);
        }

        public static TReturn InvokePrivateMethod<T, TReturn>(T obj, string name, object[] param) {
            MethodInfo field = typeof(T).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return (TReturn) field.Invoke(obj, param);
        }
    }
}
