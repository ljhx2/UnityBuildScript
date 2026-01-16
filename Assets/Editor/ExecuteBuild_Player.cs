using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

using Debug = UnityEngine.Debug;

public class ExecuteBuild_Player
{

    //[MenuItem("Build/Debug Console")]
    //public static void DebugPrintConsole()
    //{
    //}

    public enum E_BuildType
    {
        Prod,
        QA,
        Dev,
    }

    private enum E_AOSExtensionType
    {
        APK,
        AAB,
    }

    public static string CommonDefineSymbole()
    {
        HashSet<string> defines = new HashSet<string>()
        {
            "Default Symbole1",
            "Default Symbole2",
            "Default Symbole3",
        };

        return string.Join(";", defines);
    }

    private static void SettingDefineSymbole(string targetServer, NamedBuildTarget targetGroup)
    {
        string currentDefines = CommonDefineSymbole();
        HashSet<string> defines = new HashSet<string>(currentDefines.Split(';'))
        {
            targetServer
        };

        string newDefines = string.Join(";", defines);
        if (newDefines != currentDefines)
        {
            PlayerSettings.SetScriptingDefineSymbols(targetGroup, newDefines);
        }
    }
    private static void SettingDefineSymbole_Server(string targetServer)
    {
        string currentDefines = CommonDefineSymbole();
        HashSet<string> defines = new HashSet<string>(currentDefines.Split(';'))
        {
            targetServer
        };

        string newDefines = string.Join(";", defines);
        if (newDefines != currentDefines)
        {
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Server, newDefines);
        }
    }

    [MenuItem("Build/Setting Symbole/Standalone", false, 220)]
    public static void SettingDefineSymbole_Standalone_Prod()
    {
        SettingDefineSymbole("PROD", NamedBuildTarget.Standalone);
    }

    [MenuItem("Build/Setting Symbole/Server", false, 240)]
    public static void SettingDefineSymbole_Server()
    {
        SettingDefineSymbole_Server("PROD");
    }


    [MenuItem("Build/Client/Build Windows Standalone64", false, 20)]
    public static void BuildWindowsClient_Prod()
    {
        SettingDefineSymbole_Standalone_Prod();
        BuildWindowsClient(E_BuildType.Prod);
    }

    [MenuItem("Build/Client/Build Windows Standalone64 - Profiler", false, 20)]
    public static void BuildWindowsClient_ProdProfiler()
    {
        SettingDefineSymbole_Standalone_Prod();
        BuildWindowsClient(E_BuildType.Prod, true, true);
    }


    [MenuItem("Build/Client/Build Windows Standalone64", true, 20)]
    [MenuItem("Build/Client/Build Windows Standalone64 - Profiler", true, 20)]
    private static bool ValidateBuildWindowClient()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
        {
            return false;
        }
        return true;
    }

    private static void BuildWindowsClient(E_BuildType buildType, bool isLocalTest = false, bool isProfiler = false)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
        {
            Debug.LogError("Build Target Platform is not StandaloneWindows");
            Debug.LogError($"Current Build Target : {EditorUserBuildSettings.activeBuildTarget}");
            return;
        }

        //Console.Out.WriteLine("Build Windows Client Start...");
        Debug.Log("Build Windows Client Start...");

        CommonSetting();

        ExecuteBuild_Addressable.BuildAddressables();
       
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] {
            "Assets/01_Scenes/Init.unity",
        };
        string prodPath = buildType.ToString();
        string version = "VERSION" + (isProfiler ? "_Profiler" : "");
        string productName = "PRODUCT_NAME";
        buildPlayerOptions.locationPathName = $"Builds/{prodPath}/Client/Windows/{version}/{productName}.exe";
        buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;

        if (isProfiler)
        {
            buildPlayerOptions.options = BuildOptions.AutoRunPlayer | BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
        }

        BuildPipeline.BuildPlayer(buildPlayerOptions);

        Debug.Log("Build Done:" + buildPlayerOptions.locationPathName);
    }



    [MenuItem("Build/Server/Build Linux Standalone64", false, 120)]
    public static void BuildLinuxServer_Prod()
    {
        SettingDefineSymbole_Server();
        BuildLinuxServer(E_BuildType.Prod);
    }

    [MenuItem("Build/Server/Build Linux Standalone64", true, 120)]
    private static bool ValidateBuildLinuxServer()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneLinux64)
        {
            return false;
        }
        return true;
    }

    public static void BuildLinuxServer(E_BuildType buildType)
    {

#if !UNITY_SERVER
        Debug.LogError("Build Target Platform is not Server");
        return;
#else
        
        Debug.Log("Build Linux Server Start...");

        CommonSetting();

        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");


        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
        ExcuteBuild_Addressable.BuildAddressables_Server(buildType);


        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] {
            "Assets/01_Scenes/Init.unity",
        };
        string prodPath = buildType.ToString();
        buildPlayerOptions.locationPathName = $"Builds/{prodPath}/Server/Linux/{Define.VERSION}/fm002.x86_64";
        buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        //buildPlayerOptions.options = BuildOptions.EnableHeadlessMode;
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        Console.Out.WriteLine("Build Done:" + buildPlayerOptions.locationPathName);

        DeleteNodeFile(buildPlayerOptions.locationPathName);
#endif
    }


    [MenuItem("Build/Server/Build Windows Standalone64", false, 140)]
    public static void BuildWindowsServer_Prod()
    {
        SettingDefineSymbole_Server();
        BuildWindowsServer(E_BuildType.Prod);
    }


    [MenuItem("Build/Server/Build Windows Standalone64", true, 140)]
    private static bool ValidateBuildWindowsServer()
    {

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
        {
            return false;
        }
        return true;
    }

    public static void BuildWindowsServer(E_BuildType buildType)
    {

#if !UNITY_SERVER
        Debug.LogError("Build Target Platform is not Server");
        return;
#else

        Debug.Log("Build Windows Server Start...");

        CommonSetting();

        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");


        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
        ExcuteBuild_Addressable.BuildAddressables_Server(buildType);


        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] {
            "Assets/01_Scenes/Init.unity",
        };
        string prodPath = buildType.ToString();
        buildPlayerOptions.locationPathName = $"Builds/{prodPath}/Server/Windows/{Define.VERSION}/fm002.exe";
        buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        //buildPlayerOptions.options = BuildOptions.EnableHeadlessMode;
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        Console.Out.WriteLine("Build Done:" + buildPlayerOptions.locationPathName);


        DeleteNodeFile(buildPlayerOptions.locationPathName);
#endif
    }



    public static void CommonSetting()
    {
        PlayerSettings.productName = "PRODUCT_NAME";
        PlayerSettings.companyName = "COMPANY_NAME";
        PlayerSettings.bundleVersion = "VERSION";
    }

}