# AddXboxLiveConfig

A script that will automatically generate an xboxservices.config file for you, and add the config file to your output Visual Studio project automatically upon build.

For use with Universal Windows Platform applications that are using the non-Creators version of Xbox Live SDK.

If you are unsure how to get started with Xbox Live please reference [getting started with xbox live](https://docs.microsoft.com/en-us/windows/uwp/xbox-live/get-started-with-partner/create-a-new-title)

## How to use

1. Ensure you have Xbox Live enabled on your application on https://developer.microsoft.com in the developer portal dashboard
2. Find your Service Config Id and Title Id located on the Xbox Live Setup page in the developer portal.
2. Include the AddXboxLiveConfig.cs file in your Unity assets folder under Assets/Editor
3. Use the new Menu item "UWP" -> "Xbox Live" -> "Configure IDs" to input your service config ID and Title ID
4. Hit the Save button to save your configuration, you can now find this in an asset file under "Editor/XboxLiveConfig/xboxservices.config," and this will be loaded automatically next time you load the Unity editor.
5. If you build a UWP project this configuration will now be used to auto-generate an xboxservices.config file, and will be automatically included in your Visual Studio project.

## Issues

If you have issues please feel free to file a new issue and I'll try to take a look!
