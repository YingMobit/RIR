using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    public class EntitySparseArray {
        private readonly List<uint[]> sparseArrayBucket;
        private static uint[] TemplateIndexArray;

        public void SetIndex(int entityID,uint bindIndex) {
            int bucketIndex = entityID / EntityManager.ENTITY_BUCKET_SIZE;
            int indexInBucket = entityID % EntityManager.ENTITY_BUCKET_SIZE;
            while(bucketIndex >= sparseArrayBucket.Count) {
                sparseArrayBucket.Add(AllocNewPage());
            }
            sparseArrayBucket[bucketIndex][indexInBucket] = bindIndex;
        }

        public void RemoveIndex(int entityID) {
            int bucketIndex = entityID / EntityManager.ENTITY_BUCKET_SIZE;
            if(bucketIndex >= sparseArrayBucket.Count) {
                Debug.LogWarning($"The Bind Index of this Entity:{entityID} Has Not Been Setted");
                int count = bucketIndex - sparseArrayBucket.Count;
                for(; count-- > 0;) {
                    sparseArrayBucket.Add(AllocNewPage());
                }
            }
            int indexInBucket = entityID % EntityManager.ENTITY_BUCKET_SIZE;
            sparseArrayBucket[bucketIndex][indexInBucket] = 0;
        }

        public uint GetIndex(int entityID) {
            int bucketIndex = entityID / EntityManager.ENTITY_BUCKET_SIZE;
            if(bucketIndex >= sparseArrayBucket.Count) {
                Debug.LogWarning($"The Bind Index of this Entity:{entityID} Has Not Been Setted");
                int count = bucketIndex - sparseArrayBucket.Count;
                for(; count-- > 0;) {
                    sparseArrayBucket.Add(AllocNewPage());
                }
            }
            int indexInBucket = entityID % EntityManager.ENTITY_BUCKET_SIZE;
            return sparseArrayBucket[bucketIndex][indexInBucket];
        }

        private uint[] AllocNewPage() {
            uint[] newPage = new uint[EntityManager.ENTITY_BUCKET_SIZE];
            Array.Copy(TemplateIndexArray,newPage,EntityManager.ENTITY_BUCKET_SIZE);
            return newPage;
        }

        public EntitySparseArray() {
            sparseArrayBucket = new();
            sparseArrayBucket.Add(AllocNewPage());
        }

        static EntitySparseArray() {
            TemplateIndexArray = new uint[EntityManager.ENTITY_BUCKET_SIZE];
            for(int i = 0; i < EntityManager.ENTITY_BUCKET_SIZE; i++) {
                TemplateIndexArray[i] = 0;
            }
        }
    }
}