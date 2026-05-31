namespace Unity.PlatformToolkit.nintendoswitch
{
    internal class SwitchRuntimeConfiguration : BaseRuntimeConfiguration
    {
        public AttributeStore Attributes;

        public override IPlatformToolkit InstantiatePlatformToolkit()
        {
            return new SwitchPlatformToolkit(Attributes);
        }
    }
}
