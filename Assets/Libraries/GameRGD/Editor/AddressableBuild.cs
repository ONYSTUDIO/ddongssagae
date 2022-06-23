// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net.NetworkInformation;
// using System.Net.Sockets;
// using UnityEditor;
// using UnityEngine;
// using System.Net;

// #if UNITY_EDITOR
// using UnityEditor.AddressableAssets.Settings.GroupSchemas;
// using UnityEditor.AddressableAssets;
// using UnityEditor.AddressableAssets.Build;
// using UnityEditor.AddressableAssets.Settings;
// using UnityEditor.Build.Reporting;

// namespace DoubleuGames.GameRGD
// {
//     public class AddressableBuild : MonoBehaviour
//     {
//         [MenuItem("Addressable/build_Release")]
//         static void ReleaseBuild()
//         {
//             string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");

//             BuildPlayerOptions buildplayerop = new BuildPlayerOptions
//             {
//                 scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
//                 locationPathName = path,
//                 target = BuildTarget.Android,
//                 targetGroup = BuildTargetGroup.Android,
//                 options = BuildOptions.None
//             };

//             string[] levels = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

//             BuildPipeline.BuildPlayer(levels, path + "/Mediterranean.apk", BuildTarget.Android, BuildOptions.None);

//             var profileSettings = AddressableAssetSettingsDefaultObject.Settings;

//             profileSettings.activeProfileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId("Remote");

//             EditorUtility.SetDirty(profileSettings);

//             AddressableAssetSettings.BuildPlayerContent();
//         }

//         private static string[] FindEnabledEditorScenes()
//         {
//             List<string> EditorScenes = new List<string>();
//             foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
//             {
//                 if (scene.enabled)
//                 {
//                     EditorScenes.Add(scene.path);
//                 }
//             }
//             return EditorScenes.ToArray();
//         }

//         [MenuItem("Addressable/BuildAddressableAndAndroid")]
//         static void BuildAddressableAndAndroid()
//         {
//             SaveBeforeSettings();

//             BuildLocalAddressable();
//             BuildAndroid();

//             RestoreSettings();
//         }

//         [MenuItem("Addressable/BuildAddressable")]
//         static void BuildAddressable()
//         {
//             SaveBeforeSettings();

//             BuildLocalAddressable();

//             // RestoreSettings();
//         }

//         [MenuItem("Addressable/Build Android Only")]
//         static void BuildAndroidOnly()
//         {
//             SaveBeforeSettings();

//             BuildAndroid();

//             RestoreSettings();
//         }

//         [MenuItem("Addressable/Build iOS Only")]
//         static void BuildIOSOnly()
//         {
//             SaveBeforeSettings();

//             BuildIOS();

//             RestoreSettings();
//         }

//         private static string __beforeRemoteLoadPath;
//         private static string __beforeBundleVersion;

//         static void SaveBeforeSettings()
//         {
//             var _profileName = "Default";
//             var _settings = AddressableAssetSettingsDefaultObject.Settings;
//             var _profileSettings = _settings.profileSettings;
//             var _defaultProfileId = _profileSettings.GetProfileId(_profileName);

//             __beforeRemoteLoadPath = _profileSettings.GetValueByName(_defaultProfileId, "RemoteLoadPath");
//             __beforeBundleVersion = PlayerSettings.bundleVersion;
//         }

//         static void RestoreSettings()
//         {
//             var _profileName = "Default";
//             var _settings = AddressableAssetSettingsDefaultObject.Settings;
//             var _profileSettings = _settings.profileSettings;
//             var _defaultProfileId = _profileSettings.GetProfileId(_profileName);

//             _profileSettings.SetValue(_defaultProfileId, "RemoteLoadPath", __beforeRemoteLoadPath);
//             PlayerSettings.bundleVersion = __beforeBundleVersion;

//             AssetDatabase.SaveAssets();
//         }

//         static void BuildLocalAddressable()
//         {
//             var _profileName = "Default";
//             var _settings = AddressableAssetSettingsDefaultObject.Settings;
//             var _profileSettings = _settings.profileSettings;
//             var _defaultProfileId = _profileSettings.GetProfileId(_profileName);
//             var _localIpAddress = GetLocalIpAddress();
//             var _remoteLoadPath = $"http://{_localIpAddress}/[BuildTarget]";

//             _profileSettings.SetValue(_defaultProfileId, "RemoteLoadPath", _remoteLoadPath);

//             _settings.BuildRemoteCatalog = true;

//             foreach (AddressableAssetGroup _group in _settings.groups)
//             {
//                 foreach (AddressableAssetGroupSchema _schema in _group.Schemas)
//                 {
//                     if (_schema is BundledAssetGroupSchema)
//                     {
//                         var _groupSchema = _schema as BundledAssetGroupSchema;
//                         _groupSchema.LoadPath.SetVariableByName(_settings, "RemoteLoadPath");
//                         _groupSchema.BuildPath.SetVariableByName(_settings, "RemoteBuildPath");
//                         _groupSchema.RetryCount = 3;
//                         _groupSchema.UseAssetBundleCrc = false;
//                         _groupSchema.UseAssetBundleCrcForCachedBundles = false;
//                         Console.WriteLine($"[AddressableBuild:ChangeSettings] {_group.name}.{_groupSchema.name} >> retryCount: {_groupSchema.RetryCount}");
//                     }
//                 }
//             }

//             AssetDatabase.SaveAssets();

//             BuildAddressable(_profileName, false);
//         }

//         static string X_GetLocalIpAddress()
//         {
//             return NetworkInterface.GetAllNetworkInterfaces().Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
//                 .Select(x => x.GetIPProperties().UnicastAddresses.Where(y => y.Address.AddressFamily == AddressFamily.InterNetwork).Select(y => y.Address.ToString()).FirstOrDefault())
//                 .FirstOrDefault();
//         }

//         static string GetLocalIpAddress()
//         {
//             var host = null as IPHostEntry;
//             var localIp = "0.0.0.0";
//             host = Dns.GetHostEntry(Dns.GetHostName());
//             foreach (var ip in host.AddressList)
//             {
//                 if (ip.AddressFamily == AddressFamily.InterNetwork)
//                 {
//                     localIp = ip.ToString();
//                     break;
//                 }
//             }
//             return localIp;
//         }

//         static void BuildAndroid()
//         {
//             var _appName = PlayerSettings.productName;
//             var _targetDir = "./dist";
//             var _scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
//             var _buildDateTime = DateTime.Now.ToString("yyyyMMddHHmm");
//             var _fullPathAndName = $"{_targetDir}{System.IO.Path.DirectorySeparatorChar}{_appName}_{PlayerSettings.bundleVersion}_{_buildDateTime}.apk";
//             var _beforeVersion = PlayerSettings.bundleVersion;

//             PlayerSettings.bundleVersion = $"{_beforeVersion}-{_buildDateTime}";
//             AssetDatabase.SaveAssets();

//             BuildProject(_scenes, _fullPathAndName, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
//         }

//         static void BuildIOS()
//         {
//             var _appName = PlayerSettings.productName;
//             var _targetDir = "./dist";
//             var _scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
//             var _buildDateTime = DateTime.Now.ToString("yyyyMMddHHmm");
//             var _fullPathAndName = $"{_targetDir}{System.IO.Path.DirectorySeparatorChar}{_appName}_{PlayerSettings.bundleVersion}_{_buildDateTime}";
//             var _beforeVersion = PlayerSettings.bundleVersion;

//             PlayerSettings.bundleVersion = $"{_beforeVersion}.{_buildDateTime}";
//             AssetDatabase.SaveAssets();

//             BuildProject(_scenes, _fullPathAndName, BuildTargetGroup.iOS, BuildTarget.iOS, BuildOptions.None);
//         }

//         private static void BuildProject(string[] scenes, string targetDir, BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, BuildOptions buildOptions)
//         {
//             System.Console.WriteLine("[AddressableBuild] Building:" + targetDir + " buildTargetGroup:" + buildTargetGroup.ToString() + " buildTarget:" + buildTarget.ToString());

//             // https://docs.unity3d.com/ScriptReference/EditorUserBuildSettings.SwitchActiveBuildTarget.html
//             bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
//             if (switchResult)
//             {
//                 System.Console.WriteLine("[AddressableBuild] Successfully changed Build Target to: " + buildTarget.ToString());
//             }
//             else
//             {
//                 System.Console.WriteLine("[AddressableBuild] Unable to change Build Target to: " + buildTarget.ToString() + " Exiting...");
//                 return;
//             }

//             // https://docs.unity3d.com/ScriptReference/BuildPipeline.BuildPlayer.html
//             BuildReport buildReport = BuildPipeline.BuildPlayer(scenes, targetDir, buildTarget, buildOptions);
//             BuildSummary buildSummary = buildReport.summary;
//             if (buildSummary.result == BuildResult.Succeeded)
//             {
//                 System.Console.WriteLine("[AddressableBuild] Build Success: Time:" + buildSummary.totalTime + " Size:" + buildSummary.totalSize + " bytes");
//             }
//             else
//             {
//                 System.Console.WriteLine("[AddressableBuild] Build Failed: Time:" + buildSummary.totalTime + " Total Errors:" + buildSummary.totalErrors);
//             }
//         }

//         private static void BumpBundleVersion()
//         {
//             string[] lines = PlayerSettings.bundleVersion.Split('.');
//             int major = int.Parse(lines[0]);
//             int minor = lines.Length < 2 ? 0 : int.Parse(lines[1]);
//             int build = lines.Length < 3 ? 0 : int.Parse(lines[2]);
//             build++;
//             PlayerSettings.bundleVersion = $"{major}.{minor}.{build}";
//         }

//         private static void BuildAddressable(string addressableProfile, bool update = false)
//         {
//             var profileSettings = AddressableAssetSettingsDefaultObject.Settings;
//             profileSettings.activeProfileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId(addressableProfile);
//             EditorUtility.SetDirty(profileSettings);

//             if (update)
//             {
//                 ContentUpdateScript.BuildContentUpdate(profileSettings, "Assets/AddressableAssetsData/Android/addressables_content_state.bin");
//             }
//             else
//             {
//                 AddressableAssetSettings.BuildPlayerContent();
//             }
//         }
//     }
// }
// #endif
