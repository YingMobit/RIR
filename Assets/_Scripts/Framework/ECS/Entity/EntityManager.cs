using System;
using System.Collections.Generic;

namespace ECS {
    public class EntityManager {
        public const int ENTITY_BUCKET_SIZE = 64;

        private Entity[] entities;

        // ����ʵ������ջ
        private int[] freeStack;
        private int freeCount;

        // ��Ծʵ�� ID ��̬����
        private readonly List<int> activeList = new();

        // ӳ�䣺entityID -> �� activeList �е�λ�ã�δ����Ϊ -1
        private int[] activePos;

        public int ActiveEntityCount => activeList.Count;
        public int InActiveEntityCount => freeCount;
        public int TotalEntityCount => entities.Length;

        public EntityManager() {
            entities = new Entity[ENTITY_BUCKET_SIZE];
            freeStack = new int[ENTITY_BUCKET_SIZE];
            activePos = new int[ENTITY_BUCKET_SIZE];
            freeCount = 0;
            for(int i = 0; i < ENTITY_BUCKET_SIZE; i++) {
                entities[i].Set(i,-1,0,0);
                activePos[i] = -1;
                freeStack[freeCount++] = i;
            }
        }

        public Entity GetEntity(int gameObjectID,uint archetype = 0) {
            if(freeCount == 0) {
                Grow();
            }
            int id = freeStack[--freeCount];
            entities[id].Set(id,gameObjectID,entities[id].Version,archetype);
            activePos[id] = activeList.Count;
            activeList.Add(id);
            return entities[id];
        }

        public void ReleaseEntity(Entity entity) {
            int id = entity.EntityID;
            if(id < 0 || id >= entities.Length)
                return;
            int pos = activePos[id];
            if(pos < 0)
                return; // �Ѿ��ǷǼ���

            int lastIdx = activeList.Count - 1;
            if(pos != lastIdx) {
                int lastId = activeList[lastIdx];
                activeList[pos] = lastId;
                activePos[lastId] = pos;
            }
            activeList.RemoveAt(lastIdx);
            activePos[id] = -1;

            // �汾���������
            entities[id].Set(id,0,(short)(entities[id].Version + 1),0);

            // ��ֹ����(��Ҷ�����Ծ��������˵��Ҫ������)
            if(freeCount == freeStack.Length) {
                Array.Resize(ref freeStack,freeStack.Length * 2);
            }
            freeStack[freeCount++] = id;
        }

        public bool IsActive(Entity entity) {
            int id = entity.EntityID;
            if(id < 0 || id >= entities.Length)
                return false;
            return activePos[id] >= 0 && entity.Version == entities[id].Version;
        }

        public List<Entity> GetActiveEntities() {
            // ������ GC���ɸĳɴ����ⲿ List ���û��ṩö����
            var res = new List<Entity>(activeList.Count);
            for(int i = 0; i < activeList.Count; i++) {
                res.Add(entities[activeList[i]]);
            }
            return res;
        }

        private void Grow() {
            int oldLen = entities.Length;
            int newLen = oldLen + ENTITY_BUCKET_SIZE;

            Array.Resize(ref entities,newLen);
            Array.Resize(ref activePos,newLen);

            // ���� freeStack������Ҫ��
            if(freeStack.Length < newLen) {
                Array.Resize(ref freeStack,newLen);
            }

            for(int i = oldLen; i < newLen; i++) {
                entities[i].Set(i,-1,0,0);
                activePos[i] = -1;
                freeStack[freeCount++] = i;
            }
        }

        #region Internal Mutation Helpers
        internal void AddComponentMask(int entityID,uint mask) {
            ref var e = ref entities[entityID];
            e.OnAddComponent(mask);
        }

        internal void RemoveComponentMask(int entityID,uint mask) {
            ref var e = ref entities[entityID];
            e.OnRemoveComponent(mask);
        }

        internal Entity GetEntityCopy(int entityID) => entities[entityID];
        #endregion
    }
}