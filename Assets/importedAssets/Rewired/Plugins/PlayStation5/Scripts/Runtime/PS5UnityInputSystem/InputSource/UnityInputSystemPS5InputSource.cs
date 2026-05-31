// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if REWIRED_UNITY_INPUT_SYSTEM_SUPPORTED && REWIRED_UNITY_INPUT_SYSTEM_INSTALLED && REWIRED_UNITY_INPUT_SYSTEM_PS5_INSTALLED

namespace Rewired.Platforms.UnityInputSystem.PS5 {

    public partial class UnityInputSystemPS5InputSource : Rewired.Platforms.UnityInputSystem.UnityInputSystemInputSource {

        public UnityInputSystemPS5InputSource(UnityInputSystemPS5InputSourceInitOptions initOptions) : base(initOptions) {
            useApproximateMatching = false;
        }

        protected override Joystick CreateJoystick(UnityEngine.InputSystem.InputDevice device) {
            if (device == null) throw new System.ArgumentNullException("device");

            if (device is UnityEngine.InputSystem.PS5.DualSenseGamepad) {
                var gamepad = (UnityEngine.InputSystem.PS5.DualSenseGamepad)device;

                // Aim not currently supported
                //if (gamepad.isAimController) {
                //    return new PS5Aim(gamepad);
                //}

#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
                // Special device types
                UnityEngine.InputSystem.PS5.DeviceClass deviceClass = gamepad.deviceClass;
                if (deviceClass == UnityEngine.InputSystem.PS5.DeviceClass.Standard) {
                    return CreateGamepad(gamepad);
                } else {
                    // Special devices are not supported as of Unity Input System PS5 0.3.0-preview.
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

        private static PS5Gamepad CreateGamepad(UnityEngine.InputSystem.PS5.DualSenseGamepad gamepad) {
            return new PS5Gamepad(
                gamepad,
                Rewired.Platforms.UnityInputSystem.PS5.Internal.PS5Consts.sonyTerminology_gamepadName +
                " " + (gamepad.slotIndex + 1) // append player number
            );
        }

#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM

        private static PS5Gamepad CreateSpecial(UnityEngine.InputSystem.PS5.DualSenseGamepad gamepad, UnityEngine.InputSystem.PS5.DeviceClass deviceClass) {

            string tag = GetMatchingTag(deviceClass);
            if (string.IsNullOrEmpty(tag)) return null; // unsupported type

            string displayName = GetDisplayName(deviceClass);
            if (string.IsNullOrEmpty(displayName)) return null; // unsupported type

            return new PS5Gamepad(
                gamepad,
                displayName +
                " " + (gamepad.slotIndex + 1), // append player number
                tag
            );
        }

        private static string GetMatchingTag(UnityEngine.InputSystem.PS5.DeviceClass deviceClass) {
            switch(deviceClass) {
                case UnityEngine.InputSystem.PS5.DeviceClass.Standard:
                    return "PS5Gamepad";
                case UnityEngine.InputSystem.PS5.DeviceClass.Guitar:
                    return "PS5Guitar";
                case UnityEngine.InputSystem.PS5.DeviceClass.Drum:
                    return "PS5Drum";
                case UnityEngine.InputSystem.PS5.DeviceClass.DjTurntable:
                    return "PS5DjTurntable";
                case UnityEngine.InputSystem.PS5.DeviceClass.Dancemat:
                    return "PS5Dancemat";
                case UnityEngine.InputSystem.PS5.DeviceClass.Navigation:
                    return "PS5Navigation";
                case UnityEngine.InputSystem.PS5.DeviceClass.SteeringWheel:
                    return "PS5SteeringWheel";
                case UnityEngine.InputSystem.PS5.DeviceClass.Stick:
                    return "PS5Stick";
                case UnityEngine.InputSystem.PS5.DeviceClass.FlightStick:
                    return "PS5FlightStick";
                case UnityEngine.InputSystem.PS5.DeviceClass.Gun:
                    return "PS5Gun";
                default:
                    return string.Empty;
            }
        }

        private static string GetDisplayName(UnityEngine.InputSystem.PS5.DeviceClass deviceClass) {
            switch(deviceClass) {
                case UnityEngine.InputSystem.PS5.DeviceClass.Standard:
                    return Rewired.Platforms.UnityInputSystem.PS5.Internal.PS5Consts.sonyTerminology_gamepadName;
                case UnityEngine.InputSystem.PS5.DeviceClass.Guitar:
                    return Rewired.Platforms.UnityInputSystem.PS5.Internal.PS5Consts.sonyTerminology_guitarControllerName;
                case UnityEngine.InputSystem.PS5.DeviceClass.Drum:
                    return Rewired.Platforms.UnityInputSystem.PS5.Internal.PS5Consts.sonyTerminology_drumControllerName;
                case UnityEngine.InputSystem.PS5.DeviceClass.DjTurntable:
                    return string.Empty; // not supported
                case UnityEngine.InputSystem.PS5.DeviceClass.Dancemat:
                    return string.Empty; // not supported
                case UnityEngine.InputSystem.PS5.DeviceClass.Navigation:
                    return string.Empty; // not supported
                case UnityEngine.InputSystem.PS5.DeviceClass.SteeringWheel:
                    return Rewired.Platforms.UnityInputSystem.PS5.Internal.PS5Consts.sonyTerminology_steeringWheelControllerName;
                case UnityEngine.InputSystem.PS5.DeviceClass.Stick:
                    return string.Empty; // not supported
                case UnityEngine.InputSystem.PS5.DeviceClass.FlightStick:
                    return Rewired.Platforms.UnityInputSystem.PS5.Internal.PS5Consts.sonyTerminology_flightStickControllerName;
                case UnityEngine.InputSystem.PS5.DeviceClass.Gun:
                    return string.Empty; // not supported
                default:
                    return string.Empty;
            }
        }
#endif

        public interface IPS5InputDevice {
            int slotIndex { get; }
        }
    }
}

#endif
