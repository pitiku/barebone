using UnityEngine;

[CreateAssetMenu(
    fileName = "BuildAutomationSharedSettings",
    menuName = "Build/Build Automation Shared Settings")]
public sealed class BuildAutomationSharedSettings : ScriptableObject
{
    [Header("Naming Scheme (Shared)")]
    public string buildMajor = "0";
    public string buildMinor = "1";
    public string milestoneNumber = "030";
    public string buildMinorInternal = "8";

    [Header("Startup")]
    public bool applyDefineSymbolsOnEditorStartup = true;
}