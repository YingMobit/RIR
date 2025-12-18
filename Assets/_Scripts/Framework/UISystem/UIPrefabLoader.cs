using PoolingSystem.GameObjectPool;
using UnityEngine;
using Utility;

namespace UISystem { 
    public class UIPrefabLoader : Singleton<UIPrefabLoader> {
        public GameObject LoadUI(UIPrefabInfo uiPrefabInfo) {
            return GameObjectPoolCenter.Instance.GetInstance(uiPrefabInfo.UIPrefab,Vector3.zero,Quaternion.identity);
        }

        public bool TryLoadUI(UIPrefabInfo uiPrefabInfo,out GameObject gameObject) { 
            gameObject = LoadUI(uiPrefabInfo);
            return gameObject != null;
        }

        public void UnloadUI(GameObject ui) {
            GameObjectPoolCenter.Instance.ReleaseInstance(ui);
        }
    }
}