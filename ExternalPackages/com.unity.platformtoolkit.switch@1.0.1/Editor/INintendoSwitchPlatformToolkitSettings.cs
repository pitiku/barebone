using Unity.PlatformToolkit.Editor;

namespace Unity.PlatformToolkit.nintendoswitch.Editor
{
    /// <summary>
    /// Project settings for the switch Platform Toolkit Implementation.
    /// </summary>
    public interface INintendoSwitchPlatformToolkitSettings
    {
        /// <summary>
        /// The AttributeSettings used by the Switch implementation to build runtime attributes
        /// </summary>
        IAttributeSettings AttributeSettings { get; }
    }
}
