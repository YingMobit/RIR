using ReferencePoolingSystem;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;

namespace ECS{
    public class World {
        private GameObjectRegistration registration;
        private ComponentPoolManager componentPoolManager;
        private EntityManager entityManager;
        private List<EntitySparseArray> entitySparseArrays;

        private List<Query> activeQuriesCurrentFrame;
        
        #region API
        public Entity GetEntity(GameObject gameObject,uint componentTypeMask) {
            Entity newEntity = entityManager.GetEntity(registration.GetID(gameObject),componentTypeMask);
            if(componentTypeMask != 0) AddComponents(newEntity,componentTypeMask);
            return newEntity;
        }

        public void ReleaseEntity(Entity entity) {
            RemoveAllComponents(entity);
            registration.OnReleaseEntity(entity);
            entityManager.ReleaseEntity(entity);
        }

        #region GetComponents
        public List<Component> GetComponents(ComponentTypeEnum componentType) { 
            return componentPoolManager.GetComponentPool(componentType).GetAllActiveComponents();
        }

        public List<Component> GetComponents(ComponentTypeEnum componentType,out List<Entity> entities) {
            uint componentMask = componentType.ToMask();
            List<Component> result = new();
            entities = new();
            foreach(var entity in entityManager.GetActiveEntities()) {
                if((entity.Anchetype & componentMask) == componentMask) {
                    entities.Add(entity);
                    result.Add(componentPoolManager.GetComponentPool(componentType).GetActiveInstance(entitySparseArrays[(int)componentType.GetIndex()].GetIndex(entity.EntityID)));
                }
            }
            return result;
        }
        public Component GetComponentOnEntity(Entity entity,ComponentTypeEnum componentType) { 
            return componentPoolManager.GetComponentPool(componentType).GetActiveInstance(entitySparseArrays[(int)componentType.GetIndex()].GetIndex(entity.EntityID));
        }

        public List<Component> GetComponentsOnEntities(List<Entity> entities,ComponentTypeEnum componentType) { 
            List<Component> result = new();
            foreach(var entity in entities) {
                result.Add(componentPoolManager.GetComponentPool(componentType).GetActiveInstance(entitySparseArrays[(int)componentType.GetIndex()].GetIndex(entity.EntityID)));
            }
            return result;
        }


        public Query Query() { 
            var query = ReferencePoolingCenter.Instance.GetReference<Query>();
            activeQuriesCurrentFrame.Add(query);
            return query;
        }
        #endregion

        public bool AddComponent(Entity entity,ComponentTypeEnum componentType,out Component component) {
            component = componentPoolManager.GetComponentPool(componentType).GetInstance(entity,out int index);
            entitySparseArrays[(int)componentType.GetIndex()].SetIndex(entity.EntityID,index);
            entity.OnAddComponent(componentType.ToMask());
            return true;
        }

        public bool AddComponent(Entity entity,ComponentTypeEnum componentType) {
            var component = componentPoolManager.GetComponentPool(componentType).GetInstance(entity,out int index);
            entitySparseArrays[(int)componentType.GetIndex()].SetIndex(entity.EntityID,index);
            entity.OnAddComponent(componentType.ToMask());
            return true;
        }

        public bool AddComponents(Entity entity,uint componentTypeMask) { 
            
        }

        public bool RemoveComponent<TComponent>(Entity entity,ComponentTypeEnum componentType) where TComponent : Component , new() {
            var component = componentPoolManager.GetComponentPool().GetActiveInstance(entitySparseArrays[(int)componentType.GetIndex()].GetIndex(entity.EntityID));
            if(component == null) return false;
            componentPoolManager.GetComponentPool().ReleaseInstance(component,entity);
            entitySparseArrays[(int)componentType.GetIndex()].RemoveIndex(entity.EntityID);
            entity.OnRemoveComponent(componentType.ToMask());
            return true;
        }

        public bool RemoveComponents(Entity entity,uint componentTypeMask) {

        }

        public bool RemoveAllComponents(Entity entity) { 
            uint componentMask = entity.Anchetype;
            if(componentMask == 0) return false;
            return RemoveComponents(entity,componentMask);
        }

        #endregion

        #region Life Time
        public void OnUpdate(float deltaTime) { 
            // System Call
        }

        public void OnLateUpdate(float deltaTime) {
            // System Call

            foreach(var query in activeQuriesCurrentFrame) { 
                ReferencePoolingCenter.Instance.ReleaseReference(query);
            }
            activeQuriesCurrentFrame.Clear();
        }

        public void OnNetworkUpdate(int frameCount) {
            // System Call
            
        }
        #endregion

        public World() { 
            registration = new GameObjectRegistration();
            componentPoolManager = new ComponentPoolManager();
            entityManager = new EntityManager();
            entitySparseArrays = new List<EntitySparseArray>();
            activeQuriesCurrentFrame = new List<Query>();

            foreach(var componentType in System.Enum.GetValues(typeof(ComponentTypeEnum))) { 
                entitySparseArrays.Add(new EntitySparseArray());
            }
        }
    }
}