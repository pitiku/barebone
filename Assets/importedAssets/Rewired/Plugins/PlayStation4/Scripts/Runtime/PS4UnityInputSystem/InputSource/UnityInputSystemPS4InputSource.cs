// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if REWIRED_UNITY_INPUT_SYSTEM_SUPPORTED && REWIRED_UNITY_INPUT_SYSTEM_INSTALLED && REWIRED_UNITY_INPUT_SYSTEM_PS4_INSTALLED

namespace Rewired.Platforms.UnityInputSystem.PS4 {

    public partial class UnityInputSystemPS4InputSource : Rewired.Platforms.UnityInputSystem.UnityInputSystemInputSource {

        public UnityInputSystemPS4InputSource(UnityInputSystemPS4InputSourceInitOptions initOptions) : base(initOptions) {
            useApproximateMatching = false;
        }

        protected override Joystick CreateJoystick(UnityEngine.InputSystem.InputDevice device) {
            if (device == null) throw new System.ArgumentNullException("device");

            if (device is UnityEngine.InputSystem.PS4.DualShockGamepadPS4) {
                var gamepad = (UnityEngine.InputSystem.PS4.DualShockGamepadPS4)device;

                // DualShockGamepadPS4.isAimController returns true for Thrustmaster T150
                // as of testing in Unity 6000.1.0f1, Unity Input System PS4 0.2.0-pre.
                // Disable Aim Support until this is fixed.
                //if (gamepad.isAimController) {
                //    return new PS4Aim(gamepad);
                //}

#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO
                // Special device types
                UnityEngine.InputSystem.PS4.LowLevel.DeviceClass deviceClass = gamepad.deviceClass;
                if (deviceClass == UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Standard) {
                    return CreateGamepad(gamepad);
                } else {
                    // Special devices are not supported as of Unity Input System PS4 0.2.0-pre.
                    //return CreateSpecial(gamepad, deviceClass);
                    return null; // return null rather than Gamepad because axes do not work through Gamepad anyway
                }
#else
                return CreateGamepad(gamepad);
#endif
            }

            // Do not allow unsupported device types like PSMove or Unknown Controller
            return null;
        }

        private static PS4Gamepad CreateGamepad(UnityEngine.InputSystem.PS4.DualShockGamepadPS4 gamepad) {
            return new PS4Gamepad(
                gamepad,
                Rewired.Platforms.UnityInputSystem.PS4.Internal.PS4Consts.sonyTerminology_gamepadName +
                " " + (gamepad.slotIndex + 1) // append player number
            );
        }

#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO

        private static PS4Gamepad CreateSpecial(UnityEngine.InputSystem.PS4.DualShockGamepadPS4 gamepad, UnityEngine.InputSystem.PS4.LowLevel.DeviceClass deviceClass) {

            string tag = GetMatchingTag(deviceClass);
            if (string.IsNullOrEmpty(tag)) return null; // unsupported type

            string displayName = GetDisplayName(deviceClass);
            if (string.IsNullOrEmpty(displayName)) return null; // unsupported type

            return new PS4Gamepad(
                gamepad,
                displayName +
                " " + (gamepad.slotIndex + 1), // append player number
                tag
            );
        }

        private static string GetMatchingTag(UnityEngine.InputSystem.PS4.LowLevel.DeviceClass deviceClass) {
            switch(deviceClass) {
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Standard:
                    return "PS4Gamepad";
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Guitar:
                    return "PS4Guitar";
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Drum:
                    return "PS4Drum";
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.DjTurntable:
                    return "PS4DjTurntable";
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Dancemat:
                    return "PS4Dancemat";
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Navigation:
                    return "PS4Navigation";
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.SteeringWheel:
                    return "PS4SteeringWheel";
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Stick:
                    return "PS4Stick";
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.FlightStick:
                    return "PS4FlightStick";
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Gun:
                    return "PS4Gun";
                default:
                    return string.Empty;
            }
        }

        private static string GetDisplayName(UnityEngine.InputSystem.PS4.LowLevel.DeviceClass deviceClass) {
            switch(deviceClass) {
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Standard:
                    return Rewired.Platforms.UnityInputSystem.PS4.Internal.PS4Consts.sonyTerminology_gamepadName;
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Guitar:
                    return Rewired.Platforms.UnityInputSystem.PS4.Internal.PS4Consts.sonyTerminology_guitarControllerName;
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Drum:
                    return Rewired.Platforms.UnityInputSystem.PS4.Internal.PS4Consts.sonyTerminology_drumControllerName;
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.DjTurntable:
                    return string.Empty; // not supported
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Dancemat:
                    return string.Empty; // not supported
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Navigation:
                    return string.Empty; // not supported
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.SteeringWheel:
                    return Rewired.Platforms.UnityInputSystem.PS4.Internal.PS4Consts.sonyTerminology_steeringWheelControllerName;
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Stick:
                    return string.Empty; // not supported
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.FlightStick:
                    return Rewired.Platforms.UnityInputSystem.PS4.Internal.PS4Consts.sonyTerminology_flightStickControllerName;
                case UnityEngine.InputSystem.PS4.LowLevel.DeviceClass.Gun:
                    return string.Empty; // not supported
                default:
                    return string.Empty;
            }
        }
#endif

        public interface IPS4InputDevice {
            int slotIndex { get; }
        }
    }
}

#endif
