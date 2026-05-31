// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if REWIRED_UNITY_INPUT_SYSTEM_SUPPORTED && REWIRED_UNITY_INPUT_SYSTEM_INSTALLED && REWIRED_UNITY_INPUT_SYSTEM_PS4_INSTALLED

namespace Rewired.Platforms.UnityInputSystem.PS4 {

    public partial class UnityInputSystemPS4InputSource : Rewired.Platforms.UnityInputSystem.UnityInputSystemInputSource {

        protected class PS4Joystick : Joystick, IPS4InputDevice {

            private const float VIBRATION_RENEW_INTERVAL = 2.0f; // renew vibration every x seconds

            int IPS4InputDevice.slotIndex { get { return source.slotIndex; } }

            protected UnityEngine.InputSystem.PS4.DualShockGamepadPS4 source {
                get {
                    return inputDevice as UnityEngine.InputSystem.PS4.DualShockGamepadPS4;
                }
            }

            public PS4Joystick(InitOptions initOptions) : base(initOptions) {
                deviceInstanceGuid = CreateGuidHashSHA1(string.Format("{0}_{1}", _deviceName, source.slotIndex));
                AddMatchingTag(initOptions.matchingTag);
            }

            protected override void OnCreateControls(System.Collections.Generic.List<Control> controls) {
                
                // Create in fixed order
                // Add controls even if they are null to prevent index shifting if some are missing for whatever reason

                // Axes
                AddControls(source.leftStick, controls);
                AddControls(source.rightStick, controls);
                // Add triggers as axes instead of buttons for consistency with other platforms
                if (!Control.Contains(controls, source.leftTrigger)) {
                    controls.Add(new AxisControl(source.leftTrigger));
                }
                if (!Control.Contains(controls, source.rightTrigger)) {
                    controls.Add(new AxisControl(source.rightTrigger));
                }

                // Buttons
                AddControls(source.buttonSouth, controls);
                AddControls(source.buttonEast, controls);
                AddControls(source.buttonWest, controls);
                AddControls(source.buttonNorth, controls);
                AddControls(source.leftShoulder, controls);
                AddControls(source.rightShoulder, controls);
                //AddControls(source.selectButton, controls); // Share is not supported
                AddControls(source.startButton, controls);
                //AddControls(source.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("systemButton"), controls); // PS is not supported
                AddControls(source.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("touchpadButton"), controls);
                AddControls(source.leftStickButton, controls);
                AddControls(source.rightStickButton, controls);

                // D-Pad
                AddControls(source.dpad, controls);

                base.OnCreateControls(controls);
            }

            protected override bool ExcludeControl(UnityEngine.InputSystem.InputControl control) {
                if (base.ExcludeControl(control)) return true;

                // Exclude redundant buttons that activate at 0.5 for both triggers in DualShockGamepadHID
                if (control.path.EndsWith("leftTriggerButton") || control.path.EndsWith("rightTriggerButton")) {
                    return true;
                }
                return false;
            }

            protected override void OnCreateComponents(System.Collections.Generic.IList<Component> components) {
                DualMotorVibrationComponent vibrationComponent = new DualMotorVibrationComponent(this, source);
                vibrationComponent.renewVibrationInterval = VIBRATION_RENEW_INTERVAL;
                components.Add(vibrationComponent);
                base.OnCreateComponents(components);
            }

            protected override void OnCreateExtensions(System.Collections.Generic.IList<Rewired.Controller.Extension> extensions) {
                if (!Contains<Rewired.Controller.Extension, Rewired.Platforms.PS4.PS4ControllerExtension>(extensions)) {
                    extensions.Add(
                        new Rewired.Platforms.PS4.PS4ControllerExtension(
                            new PS4ControllerExtensionSource(
                                source,
                                GetComponent<DualMotorVibrationComponent>()
                            )
                        )
                    );
                }
                base.OnCreateExtensions(extensions);
            }
            
            private static bool Contains<TList, TItem>(System.Collections.Generic.IList<TList> list) where TList : class where TItem : class {
                if (list == null) return false;
                int count = list.Count;
                for (int i = 0; i < count; i++) {
                    if (list[i] as TItem != null) return true;
                }
                return false;
            }

            private static System.Guid CreateGuidHashSHA1(string text) {
                System.Guid guid;
                using (System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create()) {
                    byte[] hash = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
                    byte[] hash16 = new byte[16];
                    System.Array.Copy(hash, hash16, 16);
                    guid = new System.Guid(hash16);
                }
                return guid;
            }

            new public class InitOptions : Joystick.InitOptions {

                public string matchingTag { get; private set; }

                public InitOptions(UnityEngine.InputSystem.PS4.DualShockGamepadPS4 source, string displayName, string matchingTag)
                    : base(UnityInputSystemDeviceType.OtherJoystick, source) {
                    this.displayName = displayName;
                    this.matchingTag = matchingTag;
                }
            }
        }
    }
}

#endif
