
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DoubleuGames.GameRGD
{
    [CreateAssetMenu(menuName = "AssetsWithType")]
    public class AssetsWithType : ScriptableObject
    {
        [SerializeField] private List<AssetReference> m_Assets = null;

        public AssetReference GetAsset(int type)
        {
            if (m_Assets.Count <= type) return null;
            return m_Assets[type];
        }
    }
}
