// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if REWIRED_UNITY_INPUT_SYSTEM_SUPPORTED && REWIRED_UNITY_INPUT_SYSTEM_INSTALLED && REWIRED_UNITY_INPUT_SYSTEM_PS4_INSTALLED

namespace Rewired.Platforms.UnityInputSystem.PS4 {

    public partial class UnityInputSystemPS4InputSource : Rewired.Platforms.UnityInputSystem.UnityInputSystemInputSource {

        protected sealed class PS4Aim : PS4Joystick, IPS4InputDevice {

            private const string tag = "PS4Aim";

            public PS4Aim(UnityEngine.InputSystem.PS4.DualShockGamepadPS4 source) : base(
                new InitOptions(
                    source,
                    Rewired.Platforms.UnityInputSystem.PS4.Internal.PS4Consts.sonyTerminology_psVRControllerName + " " + (source.slotIndex + 1),
                    tag
                )
            ) {
            }

            protected override void OnCreateExtensions(System.Collections.Generic.IList<Rewired.Controller.Extension> extensions) {
                extensions.Add(
                    new Rewired.Platforms.PS4.PS4AimExtension(
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
