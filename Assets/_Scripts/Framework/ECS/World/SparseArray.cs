using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    /// <summary>
    /// 稀疏数组，ComponentID为索引，值为EntityID
    /// </summary>
    public class SparseArray
        {
        private readonly List<uint[]> sparseArrayBucket;
        private static uint[] TemplateIndexArray;

        public void SetIndex(uint componentID,uint entityIDToBind) {
            int bucketIndex = (int)componentID / ComponentPool.DEFAULT_BUCKET_CAPACITY;
            int indexInBucket = (int)componentID % ComponentPool.DEFAULT_BUCKET_CAPACITY;
            while(bucketIndex >= sparseArrayBucket.Count) {
                sparseArrayBucket.Add(AllocNewPage());
            }
            sparseArrayBucket[bucketIndex][indexInBucket] = entityIDToBind;
        }

        public void RemoveIndex(uint componentID) {
            int bucketIndex = (int)componentID / ComponentPool.DEFAULT_BUCKET_CAPACITY;
            if(bucketIndex >= sparseArrayBucket.Count) {
                Debug.LogWarning($"The Bind Index of this Entity:{componentID} Has Not Been Setted");
                int count = bucketIndex - sparseArrayBucket.Count;
                for(; count-- > 0;) {
                    sparseArrayBucket.Add(AllocNewPage());
                }
            }
            uint indexInBucket = componentID % ComponentPool.DEFAULT_BUCKET_CAPACITY;
            sparseArrayBucket[bucketIndex][indexInBucket] = 0;
        }

        public uint GetIndex(uint componentID) {
            int bucketIndex = (int)componentID / ComponentPool.DEFAULT_BUCKET_CAPACITY;
            if(bucketIndex >= sparseArrayBucket.Count) {
                Debug.LogWarning($"The Bind Index of this Component:{componentID} Has Not Been Setted");
                int count = bucketIndex - sparseArrayBucket.Count;
                for(; count-- > 0;) {
                    sparseArrayBucket.Add(AllocNewPage());
                }
            }
            uint indexInBucket = componentID % ComponentPool.DEFAULT_BUCKET_CAPACITY;
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
    }
}