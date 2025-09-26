using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS { 
    public class EntitySparseArray {
        private readonly List<int[]> sparseArrayBucket;
        private static int[] TemplateIndexArray; 

        public void SetIndex(int entityID,int bindIndex) { 
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
                Debug.LogError($"The Bind Index of this Entity:{entityID} Has Not Been Setted");
            }
            int indexInBucket = entityID % EntityManager.ENTITY_BUCKET_SIZE;
            sparseArrayBucket[bucketIndex][indexInBucket] = -1;
        }

        public int GetIndex(int entityID) { 
            int bucketIndex = entityID / EntityManager.ENTITY_BUCKET_SIZE;
            if(bucketIndex >= sparseArrayBucket.Count) { 
                Debug.LogError($"The Bind Index of this Entity:{entityID} Has Not Been Setted");
            }
            int indexInBucket = entityID % EntityManager.ENTITY_BUCKET_SIZE;
            return sparseArrayBucket[bucketIndex][indexInBucket];
        }

        private int[] AllocNewPage() {
            int[] newPage = new int[EntityManager.ENTITY_BUCKET_SIZE];
            Array.Copy(TemplateIndexArray,newPage,EntityManager.ENTITY_BUCKET_SIZE);
            return newPage;
        }

        public EntitySparseArray() {
            sparseArrayBucket = new();
            sparseArrayBucket.Add(AllocNewPage());
        }

        static EntitySparseArray() { 
            TemplateIndexArray = new int[EntityManager.ENTITY_BUCKET_SIZE];
            for(int i=0;i < EntityManager.ENTITY_BUCKET_SIZE; i++) { 
                TemplateIndexArray[i] = -1;
            }
        }
    }
}