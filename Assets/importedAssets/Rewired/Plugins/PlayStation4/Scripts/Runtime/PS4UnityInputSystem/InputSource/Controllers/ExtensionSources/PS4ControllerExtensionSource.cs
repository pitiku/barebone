// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if REWIRED_UNITY_INPUT_SYSTEM_SUPPORTED && REWIRED_UNITY_INPUT_SYSTEM_INSTALLED && REWIRED_UNITY_INPUT_SYSTEM_PS4_INSTALLED

namespace Rewired.Platforms.UnityInputSystem.PS4 {

    public partial class UnityInputSystemPS4InputSource : Rewired.Platforms.UnityInputSystem.UnityInputSystemInputSource {

        private sealed class PS4ControllerExtensionSource :
            Rewired.Platforms.PS4.Internal.IPS4GamepadExtensionSource,
            Rewired.Platforms.PS4.Internal.IPS4AimExtensionSource {

            private readonly UnityEngine.InputSystem.PS4.DualShockGamepadPS4 _source;
#if !REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO || !REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_SENSOR_INFO
            private readonly ControllerHelper _helper;
#endif

            private DualMotorVibrationComponent _dualMotorVibrationComponent;

            public PS4ControllerExtensionSource(
                UnityEngine.InputSystem.PS4.DualShockGamepadPS4 dualShockGamepadPS4,
                DualMotorVibrationComponent dualMotorVibrationComponent
            ) {
                if (dualMotorVibrationComponent == null) throw new System.ArgumentNullException("dualMotorVibrationComponent");
                _dualMotorVibrationComponent = dualMotorVibrationComponent;
                _source = dualShockGamepadPS4;
#if !REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO || !REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_SENSOR_INFO
                _helper = new ControllerHelper(dualShockGamepadPS4.slotIndex);
#endif
            }

            public bool supportsVibration { get { return true; } }

            public int vibrationMotorCount { get { return 2; } }

            public int maxTouches { get { return 2; } }

            /// <summary>
            /// Not supported.
            /// </summary>
            public int GetDeviceHandle() {
                return -1;
            }

            public UnityEngine.Vector3 GetLastAcceleration() {
                UnityEngine.Vector3 value = _source.acceleration.ReadUnprocessedValue();
                value.x *= -1f;
                value.y *= -1f;
                return value;
            }

            public UnityEngine.Vector3 GetLastAccelerationRaw() {
                return _source.acceleration.ReadUnprocessedValue();
            }

            public UnityEngine.Vector3 GetLastGyro() {
                UnityEngine.Vector3 value = _source.angularVelocity.ReadUnprocessedValue();
                value.x *= -1f;
                value.y *= -1f;
                return value;
            }

            public UnityEngine.Vector3 GetLastGyroRaw() {
                return _source.angularVelocity.ReadUnprocessedValue();
            }

            public UnityEngine.Quaternion GetLastOrientation() {
                UnityEngine.Quaternion value = _source.orientation.ReadUnprocessedValue();
                value.z *= -1f;
                value.w *= -1f;
                return value;
            }

            public UnityEngine.Quaternion GetLastOrientationRaw() {
                return _source.orientation.ReadUnprocessedValue();
            }

            public UnityEngine.Color GetUserColor() {
                return _source.lightBarColor;
            }

            public int GetUserColorId() {
                return (int)GetDeviceDescriptor().defaultColorId;
            }

            public int GetUserId() {
                return _source.ps4UserId;
            }

            public bool GetUserIsPrimary() {
                UnityEngine.PS4.PS4Input.LoggedInUser userDetails = UnityEngine.PS4.PS4Input.GetUsersDetails(_source.slotIndex);
                return userDetails.primaryUser;
            }

            public string GetUserName() {
                UnityEngine.PS4.PS4Input.LoggedInUser userDetails = UnityEngine.PS4.PS4Input.GetUsersDetails(_source.slotIndex);
                return userDetails.userName;
            }

            public int GetUserStatus() {
                UnityEngine.PS4.PS4Input.LoggedInUser userDetails = UnityEngine.PS4.PS4Input.GetUsersDetails(_source.slotIndex);
                return userDetails.status;
            }

            public float GetVibration(int motorIndex) {
                return _dualMotorVibrationComponent.GetVibration(motorIndex).intensity;
            }

            public void ResetLight() {
                _source.ResetLightBarColor();
            }

            public void ResetOrientation() {
                _source.ResetOrientation();
            }

            /// <summary>
            /// Not supported.
            /// </summary>
            public void SetAngularVelocityDeadbandState(bool enabled) {
#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_SENSOR_INFO
                _source.SetAngularVelocityDeadbandState(enabled);
#else
                _helper.SetAngularVelocityDeadbandState(enabled);
#endif
            }

            public void SetLightColor(int red, int green, int blue) {
                _source.SetLightBarColor(new UnityEngine.Color(red/255f, green/255f, blue/255f));
            }

            public void SetMotionSensorState(bool enabled) {
#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_SENSOR_INFO
                _source.SetMotionSensorState(enabled);
#else
                _helper.SetMotionSensorState(enabled);
#endif
            }

            public void SetTiltCorrectionState(bool enabled) {
#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_SENSOR_INFO
                _source.SetTiltCorrectionState(enabled);
#else
                _helper.SetTiltCorrectionState(enabled);
#endif
            }

            public void SetVibration(int motorIndex, float value) {
                _dualMotorVibrationComponent.SetVibration(new Custom.ControllerComponents.IntensityVibrationCommand() {
                    motorIndex = motorIndex,
                    vibration = new Custom.ControllerComponents.IntensityVibration(value),
                });
            }

            public void StopVibration() {
                _dualMotorVibrationComponent.StopVibration();
            }

            public int GetConnectionType() {
#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO
                return (int)_source.connectionType;
#else
                return (int)_helper.connectionType;
#endif
            }

            public int GetAnalogDeadZoneLeft() {
#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO
                // PS4 API dead zone type is byte
                return (int)(_source.stickInfo.leftStickDeadzone * 255);
#else
                return _helper.analogDeadZoneLeft;
#endif
            }

            public int GetAnalogDeadZoneRight() {
#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO
                // PS4 API dead zone type is byte
                return (int)(_source.stickInfo.rightStickDeadzone * 255);
#else
                return _helper.analogDeadZoneRight;
#endif
            }

            public float GetTouchPixelDensity() {
#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO
                return _source.touchPadInformation.pixelDensity;
#else
                return _helper.touchpixelDensity;
#endif
            }

            public int GetTouchpadResolutionX() {
#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO
                return _source.touchPadInformation.resolutionX;
#else
                return _helper.touchResolutionX;
#endif
            }

            public int GetTouchpadResolutionY() {
#if REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO
                return _source.touchPadInformation.resolutionY;
#else
                return _helper.touchResolutionY;
#endif
            }

            public int GetTouchCount() {
                int count = 0;
                var touches = _source.touches;
                for (int i = 0; i < touches.Count; i++) {
                    if (touches[i].position.value.x < 0f) continue; // returns -1,-1 when there is no touch
                    count++;
                }
                return count;
            }

            public int GetTouchId(int index) {
                if ((uint)index >= (uint)GetTouchCount()) return -1;
                return _source.touches[index].touchId.value;
            }

            public bool GetTouchPositionByIndex(int index, out UnityEngine.Vector2 position) {
                if ((uint)index >= (uint)GetTouchCount()) {
                    position = new UnityEngine.Vector2();
                    return false;
                }

                // Value is already normalized
                position = _source.touches[index].position.value;

                // Unity returns -1, -1 when there is no touch
                if (position.x < 0f) {
                    position = new UnityEngine.Vector3();
                    return false;
                }
                
                position.y = 1f - position.y; // invert y

                return true;
            }

            public bool GetTouchPositionAbsByIndex(int index, out UnityEngine.Vector2 position) {
                if (!GetTouchPositionByIndex(index, out position)) return false;

                position.x *= GetTouchpadResolutionX();
                position.y *= GetTouchpadResolutionY();

                return true;
            }

            public bool GetTouchPositionByTouchId(int touchId, out UnityEngine.Vector2 position) {
                int index = GetTouchIndexById(touchId);
                if (index < 0) {
                    position = new UnityEngine.Vector2();
                    return false;
                }
                return GetTouchPositionByIndex(index, out position);
            }

            public bool GetTouchPositionAbsByTouchId(int touchId, out UnityEngine.Vector2 position) {
                int index = GetTouchIndexById(touchId);
                if (index < 0) {
                    position = new UnityEngine.Vector2();
                    return false;
                }
                return GetTouchPositionAbsByIndex(index, out position);
            }

            public bool IsTouchingByIndex(int index) {
                if (index < 0 || index >= maxTouches) return false;
                return index < GetTouchCount();
            }

            public bool IsTouchingByTouchId(int touchId) {
                if (touchId < 0) return false;
                int index = GetTouchIndexById(touchId);
                return index >= 0;
            }

            private int GetTouchIndexById(int touchId) {
                if (touchId < 0) return -1;
                var touches = _source.touches;
                int touchNum = GetTouchCount();
                if (touchNum > 0 && touches[0].touchId.value == touchId) return 0;
                if (touchNum > 1 && touches[1].touchId.value == touchId) return 1;
                return -1;
            }

            private PS4InputDeviceDescriptor GetDeviceDescriptor() {
                return UnityEngine.JsonUtility.FromJson<PS4InputDeviceDescriptor>(_source.description.capabilities);
            }

            private struct PS4InputDeviceDescriptor {
                public uint slotId;
                public bool isAimController;
                public uint defaultColorId;
                public uint userId;

                public static PS4InputDeviceDescriptor FromJson(string json) {
                    return UnityEngine.JsonUtility.FromJson<PS4InputDeviceDescriptor>(json);
                }
            }

#if !REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_CONTROLLER_INFO || !REWIRED_PS4_UNITY_INPUT_SYSTEM_SUPPORT_SENSOR_INFO
            private class ControllerHelper {

                private int _lastUpdatedFrame = -1;

                private readonly int _slot;
                private int _touchResolutionX;
                private int _touchResolutionY;
                private int _analogDeadZoneLeft;
                private int _analogDeadZoneRight;
                private float _touchpixelDensity;
                private UnityEngine.PS4.PS4Input.ConnectionType _connectionType;

                public int slot { get { return _slot; } }
                public int touchResolutionX { get { UpdateInfo(); return _touchResolutionX; } }
                public int touchResolutionY { get { UpdateInfo(); return _touchResolutionY; } }
                public int analogDeadZoneLeft { get { UpdateInfo(); return _analogDeadZoneLeft; } }
                public int analogDeadZoneRight { get { UpdateInfo(); return _analogDeadZoneRight; } }
                public float touchpixelDensity { get { UpdateInfo(); return _touchpixelDensity; } }
                public UnityEngine.PS4.PS4Input.ConnectionType connectionType { get { UpdateInfo(); return _connectionType; } }

                public ControllerHelper(int slot) {
                    _slot = slot;
                    UpdateInfo();
                }

                public void SetAngularVelocityDeadbandState(bool enabled) {
                    if (UnityEngine.PS4.PS4Input.SpecialIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.SpecialSetAngularVelocityDeadbandState(slot, enabled);
                    } else if (UnityEngine.PS4.PS4Input.AimIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.AimSetAngularVelocityDeadbandState(slot, enabled);
                    } else if (UnityEngine.PS4.PS4Input.PadIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.PadSetAngularVelocityDeadbandState(slot, enabled);
                    }
                }

                public void SetMotionSensorState(bool enabled) {
                    if (UnityEngine.PS4.PS4Input.SpecialIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.SpecialSetMotionSensorState(slot, enabled);
                    } else if (UnityEngine.PS4.PS4Input.AimIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.AimSetMotionSensorState(slot, enabled);
                    } else if (UnityEngine.PS4.PS4Input.PadIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.PadSetMotionSensorState(slot, enabled);
                    }
                }

                public void SetTiltCorrectionState(bool enabled) {
                    if (UnityEngine.PS4.PS4Input.SpecialIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.SpecialSetTiltCorrectionState(slot, enabled);
                    } else if (UnityEngine.PS4.PS4Input.AimIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.AimSetTiltCorrectionState(slot, enabled);
                    } else if (UnityEngine.PS4.PS4Input.PadIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.PadSetTiltCorrectionState(slot, enabled);
                    }
                }

                private void UpdateInfo() {
                    if (_lastUpdatedFrame == UnityEngine.Time.frameCount) return;
                    if (UnityEngine.PS4.PS4Input.PadIsConnected(_slot)) {
                        UnityEngine.PS4.PS4Input.GetPadControllerInformation(_slot, out _touchpixelDensity, out _touchResolutionX, out _touchResolutionY, out _analogDeadZoneLeft, out _analogDeadZoneRight, out _connectionType);
                    }
                    _lastUpdatedFrame = UnityEngine.Time.frameCount;
                }
            }
#endif
        }
    }
}

#endif
