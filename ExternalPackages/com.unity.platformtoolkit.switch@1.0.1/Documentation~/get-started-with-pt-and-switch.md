# Get started with Platform Toolkit for Nintendo Switch™ or Nintendo Switch™ 2

Use the following steps to set up your Unity project for Nintendo Switch™ or Nintendo Switch™ 2 with the Platform Toolkit package.

> [!NOTE]
> At the time of release, Platform Toolkit was tested for compatibility with NintendoSDK 20.5.14.

## 1. Install the required packages

Install the following required packages:

- com.unity.platformtoolkit
- com.unity.platformtoolkit.switch

## 2. Configure project settings

1. Navigate to **Edit** > **Project Settings** > **Player Settings**.
1. Select the **Nintendo Switch** tab.
1. In **Other Settings** > **Script Compilation**, add a scripting define symbol with a value of `NN_ACCOUNT_OPENUSER_ENABLE`.
1. Select **Apply** to save the changes.

## 3. Configure Nintendo SDK Plugin

1. Import the **NintendoSDKPlugin.unitypackage** provided by Nintendo for either Nintendo Switch or Nintendo Switch 2.
1. Select the assembly definition asset to edit its properties in the Inspector. If the version of NintendoSDKPlugin doesn't come with an assembly definition file, create a new assembly definition file in the NintendoSDKPlugin folder.
1. Set the name value to`com.nintendo.sdkplugin`.
1. Add a constraint with a value of `UNITY_SWITCH || UNITY_SWITCH2 || UNITY_EDITOR || NN_PLUGIN_ENABLE`

## 4. Configure NMETA

If you use the saving system API, you must enable **Use Save Data** in the **NMETA** file. Set the minimum value to `16384` KiB.

You can access the **NMETA** file through AuthoringEditor, or from the Unity Editor by navigating to **Edit** > **Project Settings** > **Player Settings** > **Nintendo Switch** > **Publishing Settings**.

## Additional resources

* [Platform specific reference](platform-details.md)