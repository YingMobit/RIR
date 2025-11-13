using PoolingSystem.ReferencePool;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace PoolingSystem.GameObjectPool {
    public class GameObjectPoolCenter : Singleton<GameObjectPoolCenter> {
        protected override bool _isDonDestroyOnLoad => true;
        private int nextPoolID = 0;
        private List<GameObjectPool> pools = new();
        private Dictionary<int,int> PrefabID_PoolIDMap = new();

        public GameObject GetInstance(GameObject prefab,Quaternion quaternion,Transform parent = null,Action<GameObject> beforSetActive = null) { 
            int prefabID = prefab.GetInstanceID();
            GameObjectPool pool;
            if(!PrefabID_PoolIDMap.ContainsKey(prefabID)) {
                pool = ReferencePoolingCenter.Instance.GetReference<GameObjectPool>();
                var newRoot = new GameObject($"GameObjectPool_Root_{nextPoolID}_{prefab.name}");
                newRoot.transform.parent = parent;
                pool.Initialize(prefab,nextPoolID,newRoot);
                pools.Add(pool);
                PrefabID_PoolIDMap.Add(prefabID,nextPoolID);
                nextPoolID++;
            } else { 
                pool = pools[PrefabID_PoolIDMap[prefabID]];
            }
            return pool.GetInstance(quaternion,parent,beforSetActive);        
        }

        public void ReleaseInstance(GameObject instance) { 
            var marker = instance.GetComponent<BelongToPoolIDMarker>();
            if(marker != null) {
                int poolID = marker.PoolID;
                pools[poolID].ReleaseInstance(instance);
            } else {
                Debug.LogError($"Trying to release a GameObject that does not belong to any pool: {instance.name}");
                GameObject.Destroy(instance);
            }
        }
    }
}