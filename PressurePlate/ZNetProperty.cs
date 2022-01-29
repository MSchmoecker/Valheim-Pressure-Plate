using System;

namespace PressurePlate {
    public abstract class ZNetProperty<T> {
        public string Key { get; private set; }
        public T DefaultValue { get; private set; }
        protected readonly ZNetView zNetView;
        public event Action OnChange;

        protected ZNetProperty(string key, ZNetView zNetView, T defaultValue) {
            Key = key;
            DefaultValue = defaultValue;
            this.zNetView = zNetView;
        }

        private void ClaimOwnership() {
            if (!zNetView.IsOwner()) {
                zNetView.ClaimOwnership();
            }
        }

        public void ForceSet(T value) {
            ClaimOwnership();
            Set(value);
        }

        public void Set(T value) {
            SetValue(value);
            OnChange?.Invoke();
        }

        public abstract T Get();

        protected abstract void SetValue(T value);
    }

    public class BoolZNetProperty : ZNetProperty<bool> {
        public BoolZNetProperty(string key, ZNetView zNetView, bool defaultValue) : base(key, zNetView, defaultValue) {
        }

        public override bool Get() {
            return zNetView.GetZDO().GetBool(Key, DefaultValue);
        }

        protected override void SetValue(bool value) {
            zNetView.GetZDO().Set(Key, value);
        }
    }

    public class FloatZNetProperty : ZNetProperty<float> {
        public FloatZNetProperty(string key, ZNetView zNetView, float defaultValue) : base(key, zNetView, defaultValue) {
        }

        public override float Get() {
            return zNetView.GetZDO().GetFloat(Key, DefaultValue);
        }

        protected override void SetValue(float value) {
            zNetView.GetZDO().Set(Key, value);
        }

        public void ForceSet(string input) {
            Set(input);
        }

        public void Set(string input) {
            if (float.TryParse(input, out float value)) {
                Set(value);
            }
        }
    }
}
