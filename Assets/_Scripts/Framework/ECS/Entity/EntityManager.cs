using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace ECS {
    public class EntityManager {
        public const int ENTITY_BUCKET_SIZE = 64;
        public uint ActiveEntityCount => activeCount;
        public uint InActiveEntityCount => freeCount;
        public uint TotalEntityCount => freeCount + activeCount;

        private Entity[] entities;
        private uint[] freeEntityIndexStack;
        private uint[] activeEntityIndexStack;//活跃实体的ID栈
        private uint[] indexOfActiveEntityInStack;//活跃实体在ID栈中的位置
        private bool[] activeMap;
        private uint freeCount;
        private uint activeCount;

        public EntityManager() {
            entities = new Entity[ENTITY_BUCKET_SIZE];
            freeEntityIndexStack = new uint[ENTITY_BUCKET_SIZE];
            activeMap = new bool[ENTITY_BUCKET_SIZE];
            activeEntityIndexStack = new uint[ENTITY_BUCKET_SIZE];
            indexOfActiveEntityInStack = new uint[ENTITY_BUCKET_SIZE];

            for(uint i = 0; i < ENTITY_BUCKET_SIZE; i++) {
                freeEntityIndexStack[i] = i;
                indexOfActiveEntityInStack[i] = uint.MaxValue;
                entities[i].Set(i + 1,-1,0,0);
            }
            freeCount = ENTITY_BUCKET_SIZE;
            activeCount = 0;
        }

        public Entity GetEntity(int gameObjectID) {
            if(freeCount == 0) {
                Grow();
            }
            uint index = PopFreeEntityIndex();
            activeMap[index] = true;
            PushActiveEntityIndex(index);
            entities[index].Set(index + 1,gameObjectID,entities[index].Version,0);
            return entities[index];
        }

        public void ReleaseEntity(in Entity entity) {
            if(!activeMap[entity.EntityID - 1]) {
                Debug.LogError($"entity:{entity} is not active");
                return;
            }
            PushFreeEntityIndex(entity.EntityID - 1);
            PopActiveEntityIndex(entity.EntityID - 1);
            activeMap[entity.EntityID - 1] = false;
            entities[entity.EntityID - 1].Set(entity.EntityID,-1,(short)(entity.Version + 1),0);
        }

        public bool IsActive(in Entity entity) {
            return activeMap[entity.EntityID - 1];
        }

        public IEnumerable<Entity> GetActiveEntities() {
            for(int i = 0; i < activeCount; i++) {
                yield return entities[activeEntityIndexStack[i]];
            }
        }

        private void Grow() {
            uint startIndex = (uint)entities.Length;
            int newLength = entities.Length + ENTITY_BUCKET_SIZE;
            Array.Resize(ref entities,newLength);
            Array.Resize(ref freeEntityIndexStack,newLength);
            Array.Resize(ref activeMap,newLength);
            Array.Resize(ref activeEntityIndexStack,newLength);
            Array.Resize(ref indexOfActiveEntityInStack,newLength);
            for(uint i = 0; i < ENTITY_BUCKET_SIZE; i++) {
                entities[startIndex + i].Set(startIndex + i + 1,-1,0,0);
                freeEntityIndexStack[freeCount + i] = startIndex + i;
                indexOfActiveEntityInStack[startIndex + i] = uint.MaxValue;
            }
            freeCount += ENTITY_BUCKET_SIZE;
        }

        private uint PopFreeEntityIndex() {
            return freeEntityIndexStack[--freeCount];
        }

        private void PushFreeEntityIndex(uint index) {
            freeEntityIndexStack[freeCount++] = index;
        }

        private void PopActiveEntityIndex(uint index) {
            uint indexInStack = indexOfActiveEntityInStack[index];
            if(indexInStack >= activeCount) {
                Debug.LogError($"entity:{index + 1} is not in activeEntityStack");
                return;
            }
            activeEntityIndexStack[indexInStack] = activeEntityIndexStack[--activeCount];
            indexOfActiveEntityInStack[index] = uint.MaxValue;
            indexOfActiveEntityInStack[activeEntityIndexStack[indexInStack]] = indexInStack;
        }

        private void PushActiveEntityIndex(uint index) {
            indexOfActiveEntityInStack[index] = activeCount;
            activeEntityIndexStack[activeCount++] = index;
        }

        #region Internal Mutation Helpers
        internal void AddComponentMask(uint entityID,uint mask) {
            ref var e = ref entities[entityID - 1];
            e.OnAddComponent(mask);
        }

        internal void RemoveComponentMask(uint entityID,uint mask) {
            ref var e = ref entities[entityID - 1];
            e.OnRemoveComponent(mask);
        }

        internal Entity GetEntityCopy(uint entityID) => entities[entityID - 1];
        #endregion
    }
}