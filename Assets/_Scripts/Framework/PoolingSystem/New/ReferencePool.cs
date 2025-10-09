using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReferencePoolingSystem {
    public class ReferencePool {
        private List<IReference> references;
        private Stack<int> freeReferenceIndexs;
        private int totalReferenceCount = 0;
        private Type referenceType;
        private IReference ReferenceTemplate;
        private const int DEFAULT_REFERENCE_COUNT = 64;

        Type tempType;

        public void Init<TReference>() where TReference : IReference<TReference>, new() {
            referenceType = typeof(TReference);
            references = new(DEFAULT_REFERENCE_COUNT);
            freeReferenceIndexs = new(DEFAULT_REFERENCE_COUNT);
            ReferenceTemplate = new TReference();
            ExpandPool();
        }

        public TReference GetReference<TReference>() where TReference : IReference<TReference>, new() {
            tempType = typeof(TReference);
            if(tempType == referenceType) {
                if(freeReferenceIndexs.Count == 0) {
                    ExpandPool();
                }
                int freeIndex = freeReferenceIndexs.Pop();
                return (TReference)references[freeIndex];
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
            references.Capacity += DEFAULT_REFERENCE_COUNT;
            for(int i = 0; i < DEFAULT_REFERENCE_COUNT; i++) {
                var reference = ReferenceTemplate.Clone();
                reference.IndexInRefrencePool = totalReferenceCount;
                references.Add(reference);
                freeReferenceIndexs.Push(totalReferenceCount);
                totalReferenceCount++;
            }
        }
    
        public void OnDestroy() {
            foreach(var reference in references) {
                reference.OnRecycle();
            }
            references.Clear();
            freeReferenceIndexs.Clear();
            totalReferenceCount = 0;
            referenceType = null;
            ReferenceTemplate = null;
        }
    }
}