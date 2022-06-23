using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using AsyncOperation = UnityEngine.AsyncOperation;

/*
 * 어드레서블의 기본 BundledAssetProvider 에서 sync 로딩이 가능하도록 확장한 클래스
 */
[DisplayName("Assets from bundles(Async,Sync) Provider")]
public class ExtendedBundledAssetProvider : BundledAssetProvider
{
    public bool UseSyncLoad = true;

    internal class InternalOp
    {
        internal static AssetBundle LoadBundleFromDependecies(IList<object> results)
        {
            if (results == null || results.Count == 0)
                return null;

            AssetBundle bundle = null;
            bool firstBundleWrapper = true;
            for (int i = 0; i < results.Count; i++)
            {
                var abWrapper = results[i] as IAssetBundleResource;
                if (abWrapper != null)
                {
                    //only use the first asset bundle, even if it is invalid
                    var b = abWrapper.GetAssetBundle();
                    if (firstBundleWrapper)
                        bundle = b;
                    firstBundleWrapper = false;
                }
            }
            return bundle;
        }

        public void Start(ProvideHandle provideHandle)
        {
            List<object> deps = new List<object>();
            provideHandle.GetDependencies(deps);
            AssetBundle bundle = LoadBundleFromDependecies(deps);
            if (bundle == null)
            {
                provideHandle.Complete<AssetBundle>(null, false, new Exception("Unable to load dependent bundle from location " + provideHandle.Location));
                return;
            }

            object result = null;
            string subObjectName = string.Empty;
            var assetPath = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);
            if (provideHandle.Type.IsArray)
            {
                result = bundle.LoadAssetWithSubAssets(assetPath, provideHandle.Type.GetElementType());
            }
            else if (provideHandle.Type.IsGenericType && typeof(IList<>) == provideHandle.Type.GetGenericTypeDefinition())
            {
                result = bundle.LoadAssetWithSubAssets(assetPath, provideHandle.Type.GetGenericArguments()[0]);
            }
            else
            {
                if (ExtractKeyAndSubKey(assetPath, out string mainPath, out string subKey))
                {
                    subObjectName = subKey;
                    result = bundle.LoadAssetWithSubAssets(mainPath, provideHandle.Type);
                }
                else
                {
                    result = bundle.LoadAsset(assetPath, provideHandle.Type);
                }
            }

            ActionComplete(provideHandle, result, subObjectName);
        }

        private void ActionComplete(ProvideHandle provideHandle, object allAssets, string subObjectName)
        {
            object result = null;
            if (provideHandle.Type.IsArray)
            {
                result = ResourceManagerConfig.CreateArrayResult(provideHandle.Type, (UnityEngine.Object[])allAssets);
            }
            else if (provideHandle.Type.IsGenericType && typeof(IList<>) == provideHandle.Type.GetGenericTypeDefinition())
            {
                result = ResourceManagerConfig.CreateListResult(provideHandle.Type, (UnityEngine.Object[])allAssets);
            }
            else
            {
                if (string.IsNullOrEmpty(subObjectName))
                {
                    result = (allAssets != null && provideHandle.Type.IsAssignableFrom(allAssets.GetType())) ? allAssets : null;
                }
                else
                {
                    if ((UnityEngine.Object[])allAssets != null)
                    {
                        foreach (var o in (UnityEngine.Object[])allAssets)
                        {
                            if (o.name == subObjectName)
                            {
                                if (provideHandle.Type.IsAssignableFrom(o.GetType()))
                                {
                                    result = o;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            provideHandle.Complete(result, result != null, result == null ? new Exception($"Unable to load asset of type {provideHandle.Type} from location {provideHandle.Location}.") : null);
        }

        internal static bool ExtractKeyAndSubKey(object keyObj, out string mainKey, out string subKey)
        {
            var key = keyObj as string;
            if (key != null)
            {
                var i = key.IndexOf('[');
                if (i > 0)
                {
                    var j = key.LastIndexOf(']');
                    if (j > i)
                    {
                        mainKey = key.Substring(0, i);
                        subKey = key.Substring(i + 1, j - (i + 1));
                        return true;
                    }
                }
            }
            mainKey = null;
            subKey = null;
            return false;
        }
    }

    public override void Provide(ProvideHandle provideHandle)
    {
        if (UseSyncLoad)
            new InternalOp().Start(provideHandle);
        else
            base.Provide(provideHandle);
    }
}