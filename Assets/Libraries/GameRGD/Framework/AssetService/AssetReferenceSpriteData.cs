using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Helen
{
    /*
     * UISpriteData ���� ���� ���۷��� Ȯ�� Ŭ����
     * �����Ϳ��� �Է����� texture2d �� spriteatlas Ÿ�Ը��� �Ҵ��� �� ������, atlas ���� ������ �� �ִ�.
     */
    [Serializable]
    public class AssetReferenceSpriteData : AssetReference
    {
        public AssetReferenceSpriteData(string guid) : base(guid) { }

        [SerializeField]
        bool m_IsAtlasedSprite = true;
        public bool IsAtlasedSprite { get { return m_IsAtlasedSprite; } }

        private enum AssetType
        {
            NotSupported,
            Sprite,
            AtlasedSprite
        }

#if UNITY_EDITOR
        public void RefreshAtlasType()
        {
            m_IsAtlasedSprite = CheckAtlasedSprite(editorAsset) == AssetType.AtlasedSprite;
        }

        public override bool SetEditorAsset(UnityEngine.Object value)
        {
            var type = CheckAtlasedSprite(value);
            m_IsAtlasedSprite = type == AssetType.AtlasedSprite;
            if (type == AssetType.NotSupported)
            {
                Log.Warning("SpriteData can be a Sprite(Texture2d) or SpriteAtlas only.");
                return false;
            }

            return base.SetEditorAsset(value);
        }

        private AssetType CheckAtlasedSprite(UnityEngine.Object value)
        {
            // if (null == value)
            //     return AssetType.NotSupported;

            // Type type = value.GetType();
            // if (type == typeof(SpriteAtlas))
            // {
            //     return AssetType.AtlasedSprite;
            // }
            // else
            // {
            //     if (false == typeof(Texture2D).IsAssignableFrom(type))
            //         return AssetType.NotSupported;

            //     var assetPath = AssetDatabase.GetAssetOrScenePath(value);
            //     var atlasList = AssetDatabase.FindAssets("", new string[] { Const.BundleAtlasDirPath.TrimEnd('/') });
            //     foreach (var atlas in atlasList)
            //     {
            //         var toPath = AssetDatabase.GUIDToAssetPath(atlas);
            //         SpriteAtlas loadedAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(toPath);
            //         if (null == loadedAtlas)
            //             continue;

            //         var findSprite = loadedAtlas.GetSprite(Path.GetFileNameWithoutExtension(assetPath));
            //         if(findSprite != null )
            //         {
            //             var findSpritePath = AssetDatabase.GetAssetPath(findSprite.texture);
            //             if (findSpritePath.Equals(assetPath))
            //                 return AssetType.AtlasedSprite;
            //         }


            //         //if (null != loadedAtlas.GetSprite(Path.GetFileNameWithoutExtension(assetPath)))
            //         //    return AssetType.AtlasedSprite;
            //     }

            //     var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            //     return (importer != null) && (importer.spriteImportMode != SpriteImportMode.None) ?
            //         AssetType.Sprite : AssetType.NotSupported;
            // }

            return default;
        }
#endif
    }
}
