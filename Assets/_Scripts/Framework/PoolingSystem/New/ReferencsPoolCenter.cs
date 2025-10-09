using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace ReferencePoolingSystem {
    public class ReferencePoolingCenter {
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

    public class ReferenceTypes { 
        public const uint COMPONENT_SET = 0;
        public const uint QUERY = 1;

        public const int TYPE_COUNT = 2;

        private static Type[] types = new Type[TYPE_COUNT] {
            typeof(ECS.ComponentSet), // index 0
            typeof(ECS.Query), // index 1
        };
        private static Type tempType;

        public static int GetReferenceTypeIndex<TReference>() where TReference : IReference<TReference> , new() {
            tempType = typeof(TReference);
            for(int i=0;i < TYPE_COUNT; i++) {
                if(types[i] == tempType)
                    return i;
            }
            return -1;
        }
    }
}