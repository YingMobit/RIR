using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    public class EntityManager {
        public const int ENTITY_BUCKET_SIZE = 64;

        private Entity[] entities;
        private readonly SortedSet<int> activeEntityIndex = new();
        private readonly SortedSet<int> inActiveEntityIndex = new();

        public int ActiveEntityCount => activeEntityIndex.Count;
        public int InActiveEntityCount => inActiveEntityIndex.Count;
        public int TotalEntityCount => entities.Length;

        public Entity GetEntity(int gameObjectID,uint anchetype = 0) {
            if(InActiveEntityCount == 0) {
                Grow();
            }
            int index = inActiveEntityIndex.Min;
            inActiveEntityIndex.Remove(index);
            activeEntityIndex.Add(index);
            entities[index].Set(index,gameObjectID,entities[index].Version,anchetype);
            return entities[index];
        }

        public void ReleaseEntity(Entity entity) {
            activeEntityIndex.Remove(entity.EntityID);
            inActiveEntityIndex.Add(entity.EntityID);
            entities[entity.EntityID].Set(entity.EntityID,0,(short)(entity.Version+1),0);
        }

        public bool IsActive(Entity entity) { 
            return entity.Version == entities[entity.EntityID].Version && activeEntityIndex.Contains(entity.EntityID);
        }

        public List<Entity> GetActiveEntities() {
            var res = new List<Entity>(activeEntityIndex.Count);
            foreach(var index in activeEntityIndex) { 
                res.Add(entities[index]);
            }
            return res;
        }

        private void Grow() {
            Array.Resize(ref entities,entities.Length + ENTITY_BUCKET_SIZE);
            for(int i=TotalEntityCount - ENTITY_BUCKET_SIZE;i < TotalEntityCount; i++) { 
                entities[i].Set(i,-1,0,0);
                inActiveEntityIndex.Add(i);
            }
        }

        public EntityManager() { 
            entities = new Entity[ENTITY_BUCKET_SIZE];
            for(int i=0;i < ENTITY_BUCKET_SIZE; i++) { 
                entities[i].Set(i,-1,0,0);
                inActiveEntityIndex.Add(i);
            }
        }
    }
}