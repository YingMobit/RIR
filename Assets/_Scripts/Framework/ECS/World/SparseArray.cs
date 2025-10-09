using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    public class SparseArray {
        private readonly List<uint[]> sparseArrayBucket;
        private static uint[] TemplateIndexArray;

        public void SetIndex(uint indexID,uint bindID) {
            int bucketIndex = (int)indexID / ComponentPool.DEFAULT_BUCKET_CAPACITY;
            int indexInBucket = (int)indexID % ComponentPool.DEFAULT_BUCKET_CAPACITY;
            while(bucketIndex >= sparseArrayBucket.Count) {
                sparseArrayBucket.Add(AllocNewPage());
            }
            sparseArrayBucket[bucketIndex][indexInBucket] = bindID;
        }

        public void RemoveIndex(uint indexID) {
            int bucketIndex = (int)indexID / ComponentPool.DEFAULT_BUCKET_CAPACITY;
            if(bucketIndex >= sparseArrayBucket.Count) {
                Debug.LogError($"The Bind ID of this Index:{indexID} Has Not Been Setted");
                return;
            }
            uint indexInBucket = indexID % ComponentPool.DEFAULT_BUCKET_CAPACITY;
            sparseArrayBucket[bucketIndex][indexInBucket] = 0;
        }

        public uint GetIndex(uint indexID) {
            int bucketIndex = (int)indexID / ComponentPool.DEFAULT_BUCKET_CAPACITY;
            if(bucketIndex >= sparseArrayBucket.Count) {
                Debug.LogWarning($"The Bind Index of this Component:{indexID} Has Not Been Setted");
                int count = bucketIndex - sparseArrayBucket.Count;
                for(; count-- > 0;) {
                    sparseArrayBucket.Add(AllocNewPage());
                }
            }
            uint indexInBucket = indexID % ComponentPool.DEFAULT_BUCKET_CAPACITY;
            return sparseArrayBucket[bucketIndex][indexInBucket];
        }

        private uint[] AllocNewPage() {
            uint[] newPage = new uint[ComponentPool.DEFAULT_BUCKET_CAPACITY];
            Array.Copy(TemplateIndexArray,newPage,ComponentPool.DEFAULT_BUCKET_CAPACITY);
            return newPage;
        }

        public SparseArray() {
            sparseArrayBucket = new();
            sparseArrayBucket.Add(AllocNewPage());
        }

        static SparseArray() {
            TemplateIndexArray = new uint[ComponentPool.DEFAULT_BUCKET_CAPACITY];
        }

        public void OnDestroy() {
            foreach(var array in sparseArrayBucket) { 
                Array.Clear(array,0,array.Length);
            }
            sparseArrayBucket.Clear();
            Array.Clear(TemplateIndexArray,0,TemplateIndexArray.Length);
            TemplateIndexArray = null;
        }
    }
}