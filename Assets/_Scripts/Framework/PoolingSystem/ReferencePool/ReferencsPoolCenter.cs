using System;
using UnityEngine;
using Utility;

namespace PoolingSystem.ReferencePool {
    public class ReferencePoolingCenter : Singleton<ReferencePoolingCenter> {
        protected override bool _isDonDestroyOnLoad => true;
        private ReferencePool[] referencePools = new ReferencePool[ReferenceTypes.TYPE_COUNT];

        public TReference GetReference<TReference>() where TReference : IReference<TReference>, new() {
            int index = ReferenceTypes.GetReferenceTypeIndex<TReference>();
            if(index == -1) {
                Debug.LogError($"Type {typeof(TReference)} is not registered in ReferenceTypes.");
                return default;
            }
            if(referencePools[index] == null) { 
                referencePools[index] = new ReferencePool();
                referencePools[index].Init<TReference>();
            }
            return referencePools[index].GetReference<TReference>();
        }

        public void ReleaseReference<TReference>(IReference<TReference> reference) where TReference : IReference<TReference>, new() {
            int index = ReferenceTypes.GetReferenceTypeIndex<TReference>();
            if(index == -1) {
                Debug.LogError($"Type {typeof(TReference)} is not registered in ReferenceTypes.");
                return;
            }
            if(referencePools[index] == null) {
                Debug.LogError($"Reference Pool of Type {typeof(TReference)} is not initialized.");
            }
            referencePools[index].Recycle(reference);
        }

        public void OnDestroy() {
            foreach(var pool in referencePools) {
                pool.OnDestroy();
            }
            Array.Clear(referencePools, 0, referencePools.Length);
            referencePools = null;
        }
    }
}