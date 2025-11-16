using PoolingSystem.ReferencePool;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoolingSystem.GameObjectPool {
    internal class GameObjectPool : IReference<GameObjectPool> {
        int poolID;
        GameObject prefab;
        
        LinkedList<GameObject> availableInstances;
        GameObject root;

        public void Initialize(GameObject prefab,int poolID,GameObject root,int initialCapacity = 8) {
            this.prefab = prefab;
            this.poolID = poolID;
            this.root = root;
            for (int i = 0; i < initialCapacity; i++) {
                var instance = GameObject.Instantiate(prefab);
                availableInstances.AddLast(instance);
            }
        }

        public GameObject GetInstance(Vector3 worldPosition,Quaternion quaternion,Transform transform = null,Action<GameObject> beforSetActive = null) {
            GameObject instance;
            if (availableInstances.Count == 0) {
                instance = GameObject.Instantiate(prefab,root.transform);
                var comp = instance.AddComponent<BelongToPoolIDMarker>();
                comp.PoolID = poolID;
            } else {
                instance = availableInstances.First.Value;
                availableInstances.RemoveFirst();
            }
                
            if(transform != null) {
                instance.transform.SetParent(transform);
            }
            instance.transform.position = worldPosition;
            instance.transform.rotation = quaternion;
            beforSetActive?.Invoke(instance);
            instance.SetActive(true);
            return instance;
        }

        public void ReleaseInstance(GameObject instance,Action<GameObject> afterSetActive = null) {
            instance.SetActive(false);
            instance.transform.SetParent(root.transform);
            afterSetActive?.Invoke(instance);
            availableInstances.AddLast(instance);
        }

        #region IReference
        public uint ReferenceType => ReferenceTypes.GAMEOBJECTPOOL;

        int IReference.IndexInRefrencePool { get; set; }

        public void Dispose() {
            availableInstances.Clear();
            availableInstances = null;
            prefab = null;
        }

        public IReference GetNewInstance() {
            return new GameObjectPool() {
                availableInstances = new LinkedList<GameObject>(),
                prefab = null
            };
        }

        public void OnRecycle() {
            availableInstances.Clear();
            prefab = null;
        }
        #endregion
    }
}