// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if REWIRED_UNITY_INPUT_SYSTEM_SUPPORTED && REWIRED_UNITY_INPUT_SYSTEM_INSTALLED && REWIRED_UNITY_INPUT_SYSTEM_PS5_INSTALLED

namespace Rewired.Platforms.UnityInputSystem.PS5 {

    public partial class UnityInputSystemPS5InputSource : Rewired.Platforms.UnityInputSystem.UnityInputSystemInputSource {

        protected sealed class PS5Gamepad : PS5Joystick, IPS5InputDevice {

            private const string tag = "PS5Gamepad";

            public PS5Gamepad(UnityEngine.InputSystem.PS5.DualSenseGamepad source, string displayName)
                : this(source, displayName, tag) {
            }
            public PS5Gamepad(UnityEngine.InputSystem.PS5.DualSenseGamepad source, string displayName, string tag)
                : base(new InitOptions(source, displayName, tag)) {
            }

            protected override void OnCreateExtensions(System.Collections.Generic.IList<Rewired.Controller.Extension> extensions) {
                extensions.Add(
                    new Rewired.Platforms.PS5.PS5GamepadExtension(
                        new PS5ControllerExtensionSource(
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
