using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReferencePoolingSystem {
    public class ReferencePool {
        private Stack<IReference> freeReferences;
        private HashSet<IReference> usingReferences;
        private Type referenceObjectType;
        private const int defaultCapacity = 16;

        public void Init<TReference>() where TReference : IReference<TReference> , new() {
            freeReferences = new ();
            usingReferences = new ();
            referenceObjectType = typeof(TReference);

            for(int i=0;i < defaultCapacity; i++) { 
                freeReferences.Push(new TReference());
            }
        }

        public TReference GetRefrence<TReference>() where TReference : IReference<TReference> , new() {
            if(typeof(TReference) == referenceObjectType) {
                if(freeReferences.Count == 0) {
                    ExpandPool<TReference>();
                }
                var refrence = freeReferences.Pop();
                usingReferences.Add(refrence);
                return (TReference)refrence;
            } else { 
                Debug.LogError($"RefrencePool GetRefrence Type Error, Current Type is {referenceObjectType}, But Get Type is {typeof(TReference)}");
                return default;
            }
        }

        public void Recycle(IReference refrence) {
            if(usingReferences.Contains(refrence)) {
                refrence.OnRecycle();
                freeReferences.Push(refrence);
                usingReferences.Remove(refrence);
            } else { 
                Debug.LogError($"RefrencePool Recycle Error, This Refrence Not Belong To This Pool, Type is {referenceObjectType}");
            }
        }

        private void ExpandPool<TReference>() where TReference : IReference<TReference> , new() {
            int count = Mathf.Max(1, usingReferences.Count / 2);
            for(int i = 0; i < count; i++) {
                var refrence = new TReference();
                freeReferences.Push(refrence);
            }
        }

    }
}