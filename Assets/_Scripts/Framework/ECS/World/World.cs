using System.Collections.Generic;
using System.ComponentModel;
using ReferencePoolingSystem;
using Unity.Entities;
using UnityEngine;

namespace ECS {
    public class World {
        private GameObjectRegistration registration;
        private ComponentPoolManager componentPoolManager;
        private EntityManager entityManager;
        private List<EntitySparseArray> entitySparseArrays;

        private List<Query> activeQuriesCurrentFrame;

        #region API
        public Entity GetEntity(GameObject gameObject,uint componentTypeMask) {
            Entity newEntity = entityManager.GetEntity(registration.GetID(gameObject));
            if(componentTypeMask != 0)
                AddComponents(newEntity,componentTypeMask);
            return entityManager.GetEntityCopy(newEntity.EntityID);
        }

        /// <summary>
        /// 返回当前最新的实体副本（根据内部存储）。用于外部在调用 Add/Remove 后刷新本地缓存的 Entity 结构体。
        /// </summary>
        public Entity GetLatestEntity(int entityID) => entityManager.GetEntityCopy(entityID);

        public Entity GetLatestEntity(Entity entity) {
            return entityManager.GetEntityCopy(entity.EntityID);
        }

        public void ReleaseEntity(Entity entity) {
            RemoveAllComponents(entity);
            registration.OnReleaseEntity(entity);
            entityManager.ReleaseEntity(entity);
        }

        #region GetComponents
        public void GetComponents(ComponentTypeEnum componentType,out List<Component> components) {
            components = componentPoolManager.GetComponentPool(componentType).GetAllActiveComponents();
        }

        public void GetComponents(ComponentTypeEnum componentType,in List<Component> components,in List<Entity> entities) {
            uint componentMask = componentType.ToMask();
            foreach(var entity in entityManager.GetActiveEntities()) {
                if((entity.Archetype & componentMask) == componentMask) {
                    entities.Add(entity);
                    components.Add(componentPoolManager.GetComponentPool(componentType).GetActiveInstance(entitySparseArrays[(int)componentType.GetIndex()].GetIndex(entity.EntityID)));
                }
            }
        }
        public void GetComponentOnEntity(Entity entity,ComponentTypeEnum componentType,out Component component) {
            component = componentPoolManager.GetComponentPool(componentType).GetActiveInstance(entitySparseArrays[(int)componentType.GetIndex()].GetIndex(entity.EntityID));
        }

        public void GetComponentsOnEntities(List<Entity> entities,ComponentTypeEnum componentType,in List<Component> components) {
            foreach(var entity in entities) {
                components.Add(componentPoolManager.GetComponentPool(componentType).GetActiveInstance(entitySparseArrays[(int)componentType.GetIndex()].GetIndex(entity.EntityID)));
            }
        }


        public Query Query() {
            var query = ReferencePoolingCenter.Instance.GetReference<Query>();
            query.BindWorld(this);
            activeQuriesCurrentFrame.Add(query);
            return query;
        }
        #endregion

        #region AddComponent (值传递实现，通过 EntityManager 间接修改真实实体)
        public bool AddComponent(Entity entity,ComponentTypeEnum componentType,out Component component) {
            if(entity.HasComponent(componentType)) {
                GetComponentOnEntity(entity,componentType,out component);
                return true;
            }
            component = componentPoolManager.GetComponentPool(componentType).GetInstance(entity,out uint index);
            if(index == 0)
                Debug.LogError("Here");
            entitySparseArrays[(int)componentType.GetIndex()].SetIndex(entity.EntityID,index);
            entityManager.AddComponentMask(entity.EntityID,componentType.ToMask());
            return true;
        }

        public bool AddComponent(Entity entity,ComponentTypeEnum componentType) {
            if(entity.HasComponent(componentType))
                return true;
            var component = componentPoolManager.GetComponentPool(componentType).GetInstance(entity,out uint index);
            if(index == 0)
                Debug.LogError("Here");
            entitySparseArrays[(int)componentType.GetIndex()].SetIndex(entity.EntityID,index);
            entityManager.AddComponentMask(entity.EntityID,componentType.ToMask());
            return true;
        }

        public bool AddComponents(Entity entity,uint componentTypeMask) {
            componentTypeMask &= ~entity.Archetype;
            if(componentTypeMask == 0)
                return true;
            var componentTypes = componentTypeMask.MaskToEnums();
            foreach(var type in componentTypes) {
                if(!AddComponent(entity,type)) {
                    Debug.LogError($"Add component failed,but some of:{componentTypeMask} has been added");
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region RemoveComponent (值传递实现)
        public bool RemoveComponent(Entity entity,ComponentTypeEnum componentType) {
            if(!entity.HasComponent(componentType)) {
                Debug.LogError($"Entity:{entity.EntityID} doesn't has this type of Component:{componentType}");
                return false;
            }
            var component = componentPoolManager.GetComponentPool(componentType).GetActiveInstance(entitySparseArrays[(int)componentType.GetIndex()].GetIndex(entity.EntityID));
            if(component == null)
                return false;
            componentPoolManager.GetComponentPool(componentType).ReleaseInstance(component,entity,entitySparseArrays[(int)componentType.GetIndex()].GetIndex(entity.EntityID));
            entitySparseArrays[(int)componentType.GetIndex()].RemoveIndex(entity.EntityID);
            entityManager.RemoveComponentMask(entity.EntityID,componentType.ToMask());
            return true;
        }

        public bool RemoveComponents(Entity entity,uint componentTypeMask) {
            if(!entity.HasAllComponents(componentTypeMask)) {
                Debug.LogError($"Entity doesn't has all of these type of Components:{componentTypeMask}");
                return false;
            }
            var componentTypes = componentTypeMask.MaskToEnums();
            foreach(var type in componentTypes) {
                if(!RemoveComponent(entity,type)) {
                    return false;
                }
            }
            return true;
        }

        public bool RemoveAllComponents(Entity entity) {
            uint componentMask = entity.Archetype;
            if(componentMask == 0)
                return false;
            return RemoveComponents(entity,componentMask);
        }
        #endregion

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

            for(int i = 0; i < ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT; i++) {
                entitySparseArrays.Add(new EntitySparseArray());
            }
        }
    }
}