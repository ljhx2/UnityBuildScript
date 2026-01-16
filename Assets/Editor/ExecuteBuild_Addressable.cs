using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Build;

public class ExecuteBuild_Addressable
{
    static string BUILD_PATH = $"ServerData/{EditorUserBuildSettings.activeBuildTarget}";
    static string LOAD_PATH = "https://example-addressable.s3.ap-northeast-2.amazonaws.com/targetPlatform/";

    static string SETTINGS_ASSET_PATH = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
    static string PROFILE_NAME = "Default";
    static string DATA_BUILD_PATH = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
    static string VERSION = "1.0.0.0";

    [MenuItem("Build/Addressable/Build Addressables only")]
    public static void BuildAddressables()
    {
        /* DeleteAllAddressableFile */
        //AddressableAssetSettings.CleanPlayerContent();
        try
        {
            string[] all_files = Directory.GetFiles(BUILD_PATH);
            for (int i = 0; i < all_files.Length; i++)
            {
                File.Delete(all_files[i]);
            }
            string[] all_director = Directory.GetDirectories(BUILD_PATH);
            for (int i = 0; i < all_director.Length; i++)
            {
                Directory.Delete(all_director[i], true);
            }
        }
        catch (DirectoryNotFoundException e)
        {
            Debug.Log(BUILD_PATH + " : " + e);
        }


        /* Addressable Setting */
        //AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetSettings settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(SETTINGS_ASSET_PATH) as AddressableAssetSettings;
        if (settings == null)
            Debug.LogError($"{SETTINGS_ASSET_PATH} couldn't be found or isn't a settings object.");


        /* Addressable Profile */
        AddressableAssetProfileSettings profileSettings = settings.profileSettings;
        string profileId = profileSettings.GetProfileId(PROFILE_NAME);

        string remoteBuildPath = $"{BUILD_PATH}/{VERSION}";
        string remoteLoadPath = $"{LOAD_PATH}/{VERSION}";
        profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteBuildPath, remoteBuildPath);
        profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteLoadPath, remoteLoadPath);
        profileSettings.SetDirty(AddressableAssetSettings.ModificationEvent.ProfileModified, profileSettings, true);

        settings.activeProfileId = profileId;
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.ActiveProfileSet, settings, true);


        /* Addressable Version */
        settings.OverridePlayerVersion = VERSION;
        

        /* Addressable Builder */
        IDataBuilder builder = AssetDatabase.LoadAssetAtPath<ScriptableObject>(DATA_BUILD_PATH) as IDataBuilder;
        if (builder == null)
        {
            Debug.LogError(DATA_BUILD_PATH + " couldn't be found or isn't a build script.");
            return;
        }

        int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);
        if (index > 0)
        {
            settings.ActivePlayerDataBuilderIndex = index;
        }    
        else
        {
            Debug.LogWarning($"{builder} must be added to the DataBuilders list before it can be made active. Using last run builder instead.");
            return;
        }
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.BuildSettingsChanged, settings, true);


        /* Addressable Build */
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        bool success = string.IsNullOrEmpty(result.Error);

        if (!success)
        {
            Debug.LogError("Addressables build error encountered: " + result.Error);
        }
        AssetDatabase.SaveAssets();
    }
}
