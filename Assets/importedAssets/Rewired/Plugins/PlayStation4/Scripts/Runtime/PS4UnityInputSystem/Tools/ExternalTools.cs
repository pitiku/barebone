// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if REWIRED_UNITY_INPUT_SYSTEM_SUPPORTED && REWIRED_UNITY_INPUT_SYSTEM_INSTALLED && REWIRED_UNITY_INPUT_SYSTEM_PS4_INSTALLED

namespace Rewired.Platforms.UnityInputSystem.PS4.Internal {

    public sealed class ExternalTools : ExternalToolsBase {

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Initialize() {
            ExternalToolsBase.instance = new ExternalTools();
        }

        public override Rewired.Platforms.UnityInputSystem.UnityInputSystemInputSourceBase CreateInputSource(Rewired.Platforms.UnityInputSystem.PS4.UnityInputSystemPS4InputSourceInitOptions initOptions) {
            return new Rewired.Platforms.UnityInputSystem.PS4.UnityInputSystemPS4InputSource(initOptions);
        }

        public override int GetSlotIndex(Rewired.Controller controller) {
            var extension = controller.GetExtension<Rewired.Platforms.UnityInputSystem.UnityInputSystemControllerExtension>();
            if (extension == null) return -1;
            var ps4Controller = extension.inputDevice as Rewired.Platforms.UnityInputSystem.PS4.UnityInputSystemPS4InputSource.IPS4InputDevice;
            if (ps4Controller == null) return -1;
            return ps4Controller.slotIndex;
        }
    }
}

#endif
