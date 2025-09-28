using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReferencePoolingSystem {
    public class ReferencePool {
        private List<IReference[]> referenceBuckets;
        private Stack<int> freeReferenceIndexs;
        private int totalReferenceCount = 0;
        private Type referenceType;
        private IReference ReferenceTemplate;
        private const int DEFAULT_BUCKET_CAPACITY = 16;
        private const int DEFAULT_BUCKET_COUNT = 4;

        Type tempType;

        public void Init<TReference>() where TReference : IReference<TReference>, new() {
            referenceType = typeof(TReference);
            referenceBuckets = new(DEFAULT_BUCKET_COUNT);
            freeReferenceIndexs = new(DEFAULT_BUCKET_COUNT);
            ReferenceTemplate = new TReference();
            for(int i = 0; i < DEFAULT_BUCKET_COUNT; i++) {
                ExpandPool();
            }
        }

        public TReference GetReference<TReference>() where TReference : IReference<TReference>, new() {
            tempType = typeof(TReference);
            if(tempType == referenceType) {
                if(freeReferenceIndexs.Count == 0) {
                    ExpandPool();
                }
                int freeIndex = freeReferenceIndexs.Pop();
                return (TReference)referenceBuckets[freeIndex / DEFAULT_BUCKET_CAPACITY][freeIndex % DEFAULT_BUCKET_CAPACITY];
            } else {
                Debug.LogError($"RefrencePool GetReference Type Error, Current Type is {referenceType}, But Get Type is {tempType}");
                return default;
            }
        }

        public void Recycle(IReference reference) {
            tempType = reference.GetType();
            if(tempType == referenceType) {
                int index = reference.IndexInRefrencePool;
                if(index < 0 || index >= totalReferenceCount) {
                    Debug.LogError($"Recycle index out of range: {index}");
                    return;
                }
                freeReferenceIndexs.Push(index);
                reference.OnRecycle();
            } else {
                Debug.LogError($"Type dismatch,expected type:{referenceType},current instance:{reference},type: {tempType}");
                return;
            }
        }

        private void ExpandPool() {
            IReference[] bucket = new IReference[DEFAULT_BUCKET_CAPACITY];
            for(int i = 0; i < DEFAULT_BUCKET_CAPACITY; i++) {
                bucket[i] = ReferenceTemplate.Clone();
                bucket[i].IndexInRefrencePool = totalReferenceCount;
                freeReferenceIndexs.Push(totalReferenceCount++);
            }
            referenceBuckets.Add(bucket);
        }
    }
}