// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if REWIRED_UNITY_INPUT_SYSTEM_SUPPORTED && REWIRED_UNITY_INPUT_SYSTEM_INSTALLED && REWIRED_UNITY_INPUT_SYSTEM_PS4_INSTALLED

namespace Rewired.Platforms.UnityInputSystem.PS4 {

    public partial class UnityInputSystemPS4InputSource : Rewired.Platforms.UnityInputSystem.UnityInputSystemInputSource {

        protected sealed class PS4Gamepad : PS4Joystick, IPS4InputDevice {

            private const string tag = "PS4Gamepad";

            public PS4Gamepad(UnityEngine.InputSystem.PS4.DualShockGamepadPS4 source, string displayName)
                : this(source, displayName, tag) {
            }
            public PS4Gamepad(UnityEngine.InputSystem.PS4.DualShockGamepadPS4 source, string displayName, string tag)
                : base(new InitOptions(source, displayName, tag)) {
            }

            protected override void OnCreateExtensions(System.Collections.Generic.IList<Rewired.Controller.Extension> extensions) {
                extensions.Add(
                    new Rewired.Platforms.PS4.PS4GamepadExtension(
                        new PS4ControllerExtensionSource(
                            source,
                            GetComponent<DualMotorVibrationComponent>()
                        )
                    )
                );
                base.OnCreateExtensions(extensions);
            }
        }
    }
}

#endif
