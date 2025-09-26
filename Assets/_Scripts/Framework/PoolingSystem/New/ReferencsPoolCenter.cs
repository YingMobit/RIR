using System;
using System.Collections.Generic;
using Utility;

namespace ReferencePoolingSystem { 
    public class ReferencePoolingCenter : Singleton<ReferencePoolingCenter> {
        private Dictionary<Type, ReferencePool> referencePools = new Dictionary<Type, ReferencePool>();

        public TReference GetReference<TReference>() where TReference : IReference<TReference> ,new() { 
            Type type = typeof(TReference);
            if(!referencePools.TryGetValue(type, out var pool)) {
                pool = new ReferencePool();
                pool.Init<TReference>();
                referencePools.Add(type, pool);
            }
            return pool.GetRefrence<TReference>();
        }

        public void ReleaseReference<TReference>(IReference<TReference> reference) where TReference : IReference<TReference> , new() { 
            Type type = typeof(TReference);
            if(referencePools.TryGetValue(type, out var pool)) {
                pool.Recycle(reference);
            } else { 
                UnityEngine.Debug.LogError($"RefrencePoolingSystem ReleaseRefrence Error, No Pool For Type {type}");
            }
        }
    }
}