// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if REWIRED_UNITY_INPUT_SYSTEM_SUPPORTED && REWIRED_UNITY_INPUT_SYSTEM_INSTALLED && REWIRED_UNITY_INPUT_SYSTEM_PS5_INSTALLED

namespace Rewired.Platforms.UnityInputSystem.PS5 {

    public partial class UnityInputSystemPS5InputSource : Rewired.Platforms.UnityInputSystem.UnityInputSystemInputSource {

        private sealed class PS5ControllerExtensionSource :
            Rewired.Platforms.PS5.Internal.IPS5GamepadExtensionSource {

            private readonly UnityEngine.InputSystem.PS5.DualSenseGamepad _source;
#if !REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM || !REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ANGVEL_DEADBAND_STATE
            private readonly ControllerHelper _helper;
#endif

            private DualMotorVibrationComponent _dualMotorVibrationComponent;

            public PS5ControllerExtensionSource(
                UnityEngine.InputSystem.PS5.DualSenseGamepad dualShockGamepadPS5,
                DualMotorVibrationComponent dualMotorVibrationComponent
            ) {
                if (dualMotorVibrationComponent == null) throw new System.ArgumentNullException("dualMotorVibrationComponent");
                _dualMotorVibrationComponent = dualMotorVibrationComponent;
                _source = dualShockGamepadPS5;
#if !REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM || !REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ANGVEL_DEADBAND_STATE
                _helper = new ControllerHelper(dualShockGamepadPS5.slotIndex);
#endif
                SetGamepadVibrationMode(Rewired.Platforms.PS5.PS5GamepadVibrationMode.Compatible2); // required for vibration to work
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
                return _source.ps5UserId;
            }

            public bool GetUserIsPrimary() {
                UnityEngine.PS5.PS5Input.LoggedInUser userDetails = UnityEngine.PS5.PS5Input.GetUsersDetails(_source.slotIndex);
                return userDetails.primaryUser;
            }

            public string GetUserName() {
                UnityEngine.PS5.PS5Input.LoggedInUser userDetails = UnityEngine.PS5.PS5Input.GetUsersDetails(_source.slotIndex);
                return userDetails.userName;
            }

            public int GetUserStatus() {
                UnityEngine.PS5.PS5Input.LoggedInUser userDetails = UnityEngine.PS5.PS5Input.GetUsersDetails(_source.slotIndex);
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
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ANGVEL_DEADBAND_STATE
                _source.SetAngularVelocityDeadbandState(enabled);
#else
                _helper.SetAngularVelocityDeadbandState(enabled);
#endif
            }

            public void SetLightColor(int red, int green, int blue) {
                _source.SetLightBarColor(new UnityEngine.Color(red/255f, green/255f, blue/255f));
            }

            public void SetMotionSensorState(bool enabled) {
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
                _source.SetMotionSensorState(enabled);
#else
                _helper.SetMotionSensorState(enabled);
#endif
            }

            public void SetTiltCorrectionState(bool enabled) {
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
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
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
                return (int)_source.connectionType;
#else
                return (int)_helper.connectionType;
#endif
            }

            public int GetAnalogDeadZoneLeft() {
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
                // PS5 API dead zone type is byte
                return (int)(_source.stickInfo.leftStickDeadzone * 255);
#else
                return _helper.analogDeadZoneLeft;
#endif
            }

            public int GetAnalogDeadZoneRight() {
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
                // PS5 API dead zone type is byte
                return (int)(_source.stickInfo.rightStickDeadzone * 255);
#else
                return _helper.analogDeadZoneRight;
#endif
            }

            public float GetTouchPixelDensity() {
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
                return _source.touchPadInformation.pixelDensity;
#else
                return _helper.touchpixelDensity;
#endif
            }

            public int GetTouchpadResolutionX() {
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
                return _source.touchPadInformation.resolutionX;
#else
                return _helper.touchResolutionX;
#endif
            }

            public int GetTouchpadResolutionY() {
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
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

            public void SetGamepadVibrationMode(Rewired.Platforms.PS5.PS5GamepadVibrationMode mode) {
#if REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM
                UnityEngine.InputSystem.PS5.VibrationMode vibrationMode;
                switch(mode) {
                    case Platforms.PS5.PS5GamepadVibrationMode.Compatible:
                        vibrationMode = UnityEngine.InputSystem.PS5.VibrationMode.Compatible;
                        break;
                    case Platforms.PS5.PS5GamepadVibrationMode.Compatible2:
                        vibrationMode = UnityEngine.InputSystem.PS5.VibrationMode.Compatible2;
                        break;
                    case Platforms.PS5.PS5GamepadVibrationMode.Advanced:
                        vibrationMode = UnityEngine.InputSystem.PS5.VibrationMode.Advanced;
                        break;
                    default:
                        return;
                }
                _source.SetVibrationMode(vibrationMode);                
#else
                _helper.SetVibrationMode(mode);
#endif
            }

            public bool SetTriggerEffect(Rewired.Platforms.PS5.PS5GamepadTriggerType trigger, Rewired.Platforms.PS5.IPS5GamepadTriggerEffect effect) {
                if (effect == null) return false;

                UnityEngine.InputSystem.PS5.TriggerEffectParam unityEffect = UnityEngine.InputSystem.PS5.TriggerEffectParam.Create();

                switch (trigger) {
                    case Platforms.PS5.PS5GamepadTriggerType.Left:
                        unityEffect.triggerMask = UnityEngine.InputSystem.PS5.TriggerEffectMask.L2;
                        if (!ToUnityTriggerEffect(effect, out unityEffect.left)) return false;
                        break;
                    case Platforms.PS5.PS5GamepadTriggerType.Right:
                        unityEffect.triggerMask = UnityEngine.InputSystem.PS5.TriggerEffectMask.R2;
                        if (!ToUnityTriggerEffect(effect, out unityEffect.right)) return false;
                        break;
                    default:
                        return false;
                }

                _source.SetTriggerEffect(unityEffect);
                return true;
            }

            public Rewired.Platforms.PS5.PS5GamepadTriggerEffectStates GetTriggerEffectStates() {
                // Unity Input System PS5 0.3.0-preview does not implement trigger effect states.
                // Must use old UnityEngine.PS5.PS5Input system.
                Rewired.Platforms.PS5.PS5GamepadTriggerEffectStates value = new Rewired.Platforms.PS5.PS5GamepadTriggerEffectStates();
                UnityEngine.PS5.PS5Input.GamepadTriggerEffectState left;
                UnityEngine.PS5.PS5Input.GamepadTriggerEffectState right;
                UnityEngine.PS5.PS5Input.GetGamepadTriggerEffectState(_source.slotIndex, out left, out right);
                value.leftTrigger = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectState)left;
                value.rightTrigger = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectState)right;
                return value;
            }

            private int GetTouchIndexById(int touchId) {
                if (touchId < 0) return -1;
                var touches = _source.touches;
                int touchNum = GetTouchCount();
                if (touchNum > 0 && touches[0].touchId.value == touchId) return 0;
                if (touchNum > 1 && touches[1].touchId.value == touchId) return 1;
                return -1;
            }

            private PS5InputDeviceDescriptor GetDeviceDescriptor() {
                return UnityEngine.JsonUtility.FromJson<PS5InputDeviceDescriptor>(_source.description.capabilities);
            }

            private static bool ToUnityTriggerEffect(Rewired.Platforms.PS5.IPS5GamepadTriggerEffect effect, out UnityEngine.InputSystem.PS5.TriggerEffectCommand result) {
                result = new UnityEngine.InputSystem.PS5.TriggerEffectCommand();
                if (effect == null) return false;

                switch (effect.triggerEffectType) {
                    case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.Off:
                        result.mode = UnityEngine.InputSystem.PS5.TriggerEffectMode.Off;
                        break;
                    case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.Feedback: {
                            result.mode = UnityEngine.InputSystem.PS5.TriggerEffectMode.Feedback;
                            var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectFeedback)effect;
                            result.feedback.position = tEffect.position;
                            result.feedback.strength = tEffect.strength;
                        }
                        break;
                    case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.Weapon: {
                            result.mode = UnityEngine.InputSystem.PS5.TriggerEffectMode.Weapon;
                            var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectWeapon)effect;
                            result.weapon.startPosition = tEffect.startPosition;
                            result.weapon.endPosition = tEffect.endPosition;
                            result.weapon.strength = tEffect.strength;
                        }
                        break;
                    case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.Vibration: {
                            result.mode = UnityEngine.InputSystem.PS5.TriggerEffectMode.Vibration;
                            var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectVibration)effect;
                            result.vibration.position = tEffect.position;
                            result.vibration.amplitude = tEffect.amplitude;
                            result.vibration.frequency = tEffect.frequency;
                        }
                        break;
                    case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.MultiplePositionFeedback: {
                            result.mode = UnityEngine.InputSystem.PS5.TriggerEffectMode.MultiplePositionFeedback;
                            var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectMultiplePositionFeedback)effect;
                            var strength = tEffect.strength;
                            result.multiplePositionFeedback.strength0 = strength[0];
                            result.multiplePositionFeedback.strength1 = strength[1];
                            result.multiplePositionFeedback.strength2 = strength[2];
                            result.multiplePositionFeedback.strength3 = strength[3];
                            result.multiplePositionFeedback.strength4 = strength[4];
                            result.multiplePositionFeedback.strength5 = strength[5];
                            result.multiplePositionFeedback.strength6 = strength[6];
                            result.multiplePositionFeedback.strength7 = strength[7];
                            result.multiplePositionFeedback.strength8 = strength[8];
                            result.multiplePositionFeedback.strength9 = strength[9];
                        }
                        break;
                    case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.SlopeFeedback: {
                            result.mode = UnityEngine.InputSystem.PS5.TriggerEffectMode.SlopeFeedback;
                            var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectSlopeFeedback)effect;
                            result.slopeFeedback.startPosition = tEffect.startPosition;
                            result.slopeFeedback.startStrength = tEffect.startStrength;
                            result.slopeFeedback.endPosition = tEffect.endPosition;
                            result.slopeFeedback.endStrength = tEffect.endStrength;
                        }
                        break;
                    case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.MultiplePositionVibration: {
                            result.mode = UnityEngine.InputSystem.PS5.TriggerEffectMode.MultiplePositionVibration;
                            var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectMultiplePositionVibration)effect;
                            var amplitude = tEffect.amplitude;
                            result.multiplePositionVibration.frequency = tEffect.frequency;
                            result.multiplePositionVibration.amplitude0 = amplitude[0];
                            result.multiplePositionVibration.amplitude1 = amplitude[1];
                            result.multiplePositionVibration.amplitude2 = amplitude[2];
                            result.multiplePositionVibration.amplitude3 = amplitude[3];
                            result.multiplePositionVibration.amplitude4 = amplitude[4];
                            result.multiplePositionVibration.amplitude5 = amplitude[5];
                            result.multiplePositionVibration.amplitude6 = amplitude[6];
                            result.multiplePositionVibration.amplitude7 = amplitude[7];
                            result.multiplePositionVibration.amplitude8 = amplitude[8];
                            result.multiplePositionVibration.amplitude9 = amplitude[9];
                        }
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
                return true;
            }

            private struct PS5InputDeviceDescriptor {
                public uint slotId;
                public bool isAimController;
                public uint defaultColorId;
                public uint userId;

                public static PS5InputDeviceDescriptor FromJson(string json) {
                    return UnityEngine.JsonUtility.FromJson<PS5InputDeviceDescriptor>(json);
                }
            }

#if !REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ENHANCED_INPUT_SYSTEM || !REWIRED_PS5_UNITY_INPUT_SYSTEM_SUPPORT_ANGVEL_DEADBAND_STATE
            private class ControllerHelper {

                private int _lastUpdatedFrame = -1;

                private readonly int _slot;
                private int _touchResolutionX;
                private int _touchResolutionY;
                private int _analogDeadZoneLeft;
                private int _analogDeadZoneRight;
                private float _touchpixelDensity;
                private UnityEngine.PS5.PS5Input.ConnectionType _connectionType;

                public int slot { get { return _slot; } }
                public int touchResolutionX { get { UpdateInfo(); return _touchResolutionX; } }
                public int touchResolutionY { get { UpdateInfo(); return _touchResolutionY; } }
                public int analogDeadZoneLeft { get { UpdateInfo(); return _analogDeadZoneLeft; } }
                public int analogDeadZoneRight { get { UpdateInfo(); return _analogDeadZoneRight; } }
                public float touchpixelDensity { get { UpdateInfo(); return _touchpixelDensity; } }
                public UnityEngine.PS5.PS5Input.ConnectionType connectionType { get { UpdateInfo(); return _connectionType; } }

                public ControllerHelper(int slot) {
                    _slot = slot;
                    UpdateInfo();
                }

                public void SetAngularVelocityDeadbandState(bool enabled) {
                    if (UnityEngine.PS5.PS5Input.SpecialIsConnected(_slot)) {
                        UnityEngine.PS5.PS5Input.SpecialSetAngularVelocityDeadbandState(slot, enabled);
                    } else if (UnityEngine.PS5.PS5Input.PadIsConnected(_slot)) {
                        UnityEngine.PS5.PS5Input.PadSetAngularVelocityDeadbandState(slot, enabled);
                    }
                }

                public void SetMotionSensorState(bool enabled) {
                    if (UnityEngine.PS5.PS5Input.SpecialIsConnected(_slot)) {
                        UnityEngine.PS5.PS5Input.SpecialSetMotionSensorState(slot, enabled);
                    } else if (UnityEngine.PS5.PS5Input.PadIsConnected(_slot)) {
                        UnityEngine.PS5.PS5Input.PadSetMotionSensorState(slot, enabled);
                    }
                }

                public void SetTiltCorrectionState(bool enabled) {
                    if (UnityEngine.PS5.PS5Input.SpecialIsConnected(_slot)) {
                        UnityEngine.PS5.PS5Input.SpecialSetTiltCorrectionState(slot, enabled);
                    } else if (UnityEngine.PS5.PS5Input.PadIsConnected(_slot)) {
                        UnityEngine.PS5.PS5Input.PadSetTiltCorrectionState(slot, enabled);
                    }
                }

                public void SetVibrationMode(Rewired.Platforms.PS5.PS5GamepadVibrationMode mode) {
                    UnityEngine.PS5.PS5Input.VibrationMode unityMode;
                    switch (mode) {
                        case Platforms.PS5.PS5GamepadVibrationMode.Compatible:
                            unityMode = UnityEngine.PS5.PS5Input.VibrationMode.Compatible;
                            break;
                        case Platforms.PS5.PS5GamepadVibrationMode.Compatible2:
                            unityMode = UnityEngine.PS5.PS5Input.VibrationMode.Compatible2;
                            break;
                        case Platforms.PS5.PS5GamepadVibrationMode.Advanced:
                            unityMode = UnityEngine.PS5.PS5Input.VibrationMode.Advanced;
                            break;
                        default:
                            return;
                    }
                    UnityEngine.PS5.PS5Input.PadSetVibrationMode(_slot, unityMode);
                }

                private void UpdateInfo() {
                    if (_lastUpdatedFrame == UnityEngine.Time.frameCount) return;
                    if (UnityEngine.PS5.PS5Input.PadIsConnected(_slot)) {
                        UnityEngine.PS5.PS5Input.GetPadControllerInformation(_slot, out _touchpixelDensity, out _touchResolutionX, out _touchResolutionY, out _analogDeadZoneLeft, out _analogDeadZoneRight, out _connectionType);
                    }
                    _lastUpdatedFrame = UnityEngine.Time.frameCount;
                }
            }
#endif
        }
    }
}

#endif
