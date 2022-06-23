using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;

#endif

namespace Helen
{
    public static class AddressablesUtil
    {
        public enum AssetBundlePosition
        {
            Local,
            Remote,
        }

        public enum RemoteDownloadPosition
        {
            NoDown,
            DownOnPatchScene,
            DownOnLobbyScene,
        }

#if UNITY_EDITOR

        public static bool IsPlayModeFast()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            Type currentType = settings.ActivePlayModeDataBuilder.GetType();
            return typeof(BuildScriptFastMode).IsAssignableFrom(currentType);
        }

        public static void RemoveAllGroups(bool showProgressBar)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            int maxCount = settings.groups.Count;
            for (int i = settings.groups.Count - 1; i >= 0; i--)
            {
                AddressableAssetGroup group = settings.groups[i];
                if (null == group)
                    continue;

                if (true == showProgressBar && true == EditorUtility.DisplayCancelableProgressBar("Removing groups", group.Name, (float)(maxCount - i) / (float)maxCount))
                    break;
                if (true == group.IsDefaultGroup() || AssetService.BuiltInDataGroupName == group.Name)
                    continue;
                settings.RemoveGroup(group);
            }
            if (true == showProgressBar)
                EditorUtility.ClearProgressBar();
        }

        public static void DeleteBuildedRemoteBundles()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            string remoteBuildPath = settings.RemoteCatalogBuildPath.GetValue(settings);
            if (true == Directory.Exists(remoteBuildPath))
                Directory.Delete(remoteBuildPath, true);
        }

        public static string GetBackupContentStateFilePath(string assetBundleWorkPath)
        {
            string csPath = assetBundleWorkPath + "/ContentState";
            return Path.Combine(csPath, "addressables_content_state.bin");
        }

        public static void CreateLocalBundleUpdateGroup(string assetBundleWorkPath)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            string path = GetBackupContentStateFilePath(assetBundleWorkPath);
            var entries = ContentUpdateScript.GatherModifiedEntries(settings, path);
            var UpdateGroup = new List<AddressableAssetEntry>();

            foreach (var entry in entries)
            {
                if (entry.labels.Contains(RemoteDownloadPosition.NoDown.ToString()))
                    continue;

                var labelCopy = entry.labels.ToList();
                foreach (var label in labelCopy)
                    entry.SetLabel(label, false);

                entry.SetLabel(AddressablesUtil.AssetBundlePosition.Remote.ToString(), true, true);
                entry.SetLabel(AddressablesUtil.RemoteDownloadPosition.DownOnPatchScene.ToString(), true, true);

                UpdateGroup.Add(entry);
            }

            ContentUpdateScript.CreateContentUpdateGroup(settings, UpdateGroup, "Content Update");
        }

        public static AddressablesPlayerBuildResult BuildContentUpdate(string assetBundleWorkPath)
        {
            string path = GetBackupContentStateFilePath(assetBundleWorkPath);
            return ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
        }

        public static void BackupContentStateFile(string assetBundleWorkPath)
        {
            if (string.IsNullOrEmpty(assetBundleWorkPath))
                return;
            string csPath = assetBundleWorkPath + "/ContentState";
            if (false == Directory.Exists(csPath))
                Directory.CreateDirectory(csPath);
            string path = ContentUpdateScript.GetContentStateDataPath(false);
            string targetPath = Path.Combine(csPath, "addressables_content_state.bin");
            File.Copy(path, targetPath, true);
        }

        [MenuItem("Tools/AddressableUtil/Backup Local Bundle")]
        public static void BackupLocalBundleFile()
        {
            string target = "../TestLocalBundle";
            BackupLocalBundleFile(target);
        }

        public static void BackupLocalBundleFile(string assetBundleWorkPath)
        {
            var dest = assetBundleWorkPath + "/LocalBundle";
            var source = UnityEngine.AddressableAssets.Addressables.BuildPath;

            if (Directory.Exists(dest))
                Directory.Delete(dest, true);

            if (!Directory.Exists(source))
                return;

            CopyFolder(source, dest);
        }


        [MenuItem("Tools/AddressableUtil/Load Local Bundle")]
        public static void LoadLocalBundleFile()
        {
            string target = "../TestLocalBundle";
            LoadLocalBundleFile(target);
        }

        public static void LoadLocalBundleFile(string assetbundleWorkPath)
        {
            var source = assetbundleWorkPath + "/LocalBundle";
            var dest = UnityEngine.AddressableAssets.Addressables.BuildPath;

            if (Directory.Exists(dest))
                Directory.Delete(dest, true);

            if (!Directory.Exists(source))
                return;

            CopyFolder(source, dest);
        }


        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            string[] files = Directory.GetFiles(sourceFolder);
            string[] folders = Directory.GetDirectories(sourceFolder);

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
            }

            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }


        public static void ClearPreBundleFiles(string assetBundleWorkPath)
        {
            if (string.IsNullOrEmpty(assetBundleWorkPath))
                return;

            if (false == Directory.Exists(assetBundleWorkPath))
                return;

            Log.Trace($"Start clear Pre Bundle Files : {assetBundleWorkPath}");

            var files = Directory.GetFiles(assetBundleWorkPath);
            foreach (var file in files)
            {
                Log.Trace($"Clear Pre Bundle Files, delete File : {file}");
                File.Delete(file);
            }

            Log.Trace($"End clear Pre Bundle Files : {assetBundleWorkPath}");

        }

        public static AddressableAssetGroupTemplate LoadGroupTemplate(AddressablesUtil.AssetBundlePosition Position)
        {
            string fileName;
            switch (Position)
            {
                default:
                case AddressablesUtil.AssetBundlePosition.Local:
                    fileName = "Local Assets.asset";
                    break;
                case AddressablesUtil.AssetBundlePosition.Remote:
                    fileName = "Remote Assets.asset";
                    break;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            string groupTemplatePath = PathHelper.Combine(settings.GroupTemplateFolder, fileName);
            var template = AssetDatabase.LoadAssetAtPath(groupTemplatePath, typeof(ScriptableObject));
            if (null == template)
                Log.Error("Cannot load addressable group template :" + fileName);
            return template as AddressableAssetGroupTemplate;
        }

        [UnityEditor.MenuItem("Tools/AddressableUtil/PatchGroupSetRemote")]
        public static void PatchGroupSetRemote()
        {
            PatchGroupSetStaticContent(false);
        }

        [UnityEditor.MenuItem("Tools/AddressableUtil/PatchGroupSetLocal")]
        public static void PatchGroupSetLocal()
        {
            PatchGroupSetStaticContent(true);
        }

        public static void PatchGroupSetStaticContent(bool staticContent)
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;

            List<AddressableAssetGroup> patchGroup = new List<AddressableAssetGroup>();
            foreach (var group in setting.groups)
            {
                bool patch = false;

                foreach (var entry in group.entries)
                {
                    if (entry.labels.Contains(RemoteDownloadPosition.DownOnPatchScene.ToString()))
                    {
                        patch = true;
                        break;
                    }
                }

                if (patch)
                    patchGroup.Add(group);
            }

            var template = LoadGroupTemplate(staticContent ? AssetBundlePosition.Local : AssetBundlePosition.Remote);
            var bundleSchemaTemplate = template.GetSchemaByType(typeof(BundledAssetGroupSchema));

            foreach (var group in patchGroup)
            {
                var updateSchema = group.GetSchema<ContentUpdateGroupSchema>();
                if (updateSchema == null)
                    continue;

                var bundleSchema = group.RemoveSchema<BundledAssetGroupSchema>();
                group.AddSchema(bundleSchemaTemplate);
                updateSchema.StaticContent = staticContent;
            }
        }
#endif
    }
}
