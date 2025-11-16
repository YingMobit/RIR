using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drive;
using InputSystemNameSpace;
using PoolingSystem.ReferencePool;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEngine.Pool;

namespace ECS {
    //��ϡ������Ľṹ��Ϊ�洢EntityID
    public class World {
        private GameObjectRegistration registration;
        private ComponentPoolManager componentPoolManager;
        private EntityManager entityManager;
        private SparseArray[] entitySearchSparseArrays;
        private SparseArray[] componentSearchSparseArrays;
        private List<Query> activeQuriesCurrentFrame;

        private List<ISystem> systems;
        private InputSystem inputSystem;

        #region API
        public int GetEntityCount() => (int)entityManager.TotalEntityCount;
        public int GetActiveEntityCount() => (int)entityManager.ActiveEntityCount;
        public int GetComponentCount(ComponentTypeEnum componentType) => componentPoolManager.GetComponentPool(componentType).TotalComponentCount;
        public int GetActiveComponentCount(ComponentTypeEnum componentType) => componentPoolManager.GetComponentPool(componentType).ActiveComponentCount;

        public Entity GetEntity(GameObject gameObject,uint componentTypeMask) {
            Entity newEntity = entityManager.GetEntity(registration.GetID(gameObject));
            if(componentTypeMask != 0)
                AddComponents(newEntity,componentTypeMask);
            return entityManager.GetEntityCopy(newEntity.EntityID);
        }

        /// <summary>
        /// ���ص�ǰ���µ�ʵ�帱���������ڲ��洢���������ⲿ�ڵ��� Add/Remove ��ˢ�±��ػ���� Entity �ṹ�塣
        /// </summary>
        public Entity GetLatestEntity(uint entityID) => entityManager.GetEntityCopy(entityID);

        public Entity GetLatestEntity(Entity entity) {
            return entityManager.GetEntityCopy(entity.EntityID);
        }

        public GameObject GetGameObject(Entity entity) {
            return registration.GetGameObject(entity.GameObjectID);
        }

        public void ReleaseEntity(Entity entity) {
            RemoveAllComponents(entity);
            registration.OnReleaseEntity(entity);
            entityManager.ReleaseEntity(entity);
        }


        #region GetComponents
        public void GetComponents(ComponentTypeEnum componentType,in List<Component> components) {
            componentPoolManager.GetComponentPool(componentType).GetAllActiveComponents(components);
        }

        public void GetComponents(ComponentTypeEnum componentType,in List<Component> components,in List<Entity> entityCopies) {
            componentPoolManager.GetComponentPool(componentType).GetAllActiveComponents(components);
            int count = components.Count;
            if(entityCopies.Capacity < count)
                entityCopies.Capacity = count;
            entityCopies.Clear();
            for(int i = 0; i < count; i++) {
                entityCopies.Add(entityManager.GetEntityCopy(entitySearchSparseArrays[componentType.GetIndex()].GetIndex(components[i].ComponentID)));
            }
        }
        public void GetComponentOnEntity(Entity entity,ComponentTypeEnum componentType,out Component component) {
            component = componentPoolManager.GetComponentPool(componentType).GetActiveInstance(componentSearchSparseArrays[componentType.GetIndex()].GetIndex(entity.EntityID));
        }

        public void GetAllComponentsOnEntity(Entity entity,List<Component> components) {
            components.Clear();
            var componentTypes = entity.Archetype.MaskToEnums();
            if(components.Capacity < componentTypes.Length) {
                components.Capacity = componentTypes.Length;
            }
            foreach(var type in componentTypes) {
                GetComponentOnEntity(entity,type,out var component);
                if(component != null) {
                    components.Add(component);
                }
            }
        }

        public Query Query() {
            var query = ReferencePoolingCenter.Instance.GetReference<Query>();
            query.BindWorld(this);
            activeQuriesCurrentFrame.Add(query);
            return query;
        }
        #endregion

        #region AddComponent (ֵ����ʵ�֣�ͨ�� EntityManager ����޸���ʵʵ��)
        public bool AddComponent(Entity entity,ComponentTypeEnum componentType,out Component component) {
            if(entity.HasComponent(componentType)) {
                GetComponentOnEntity(entity,componentType,out component);
                return true;
            }
            component = componentPoolManager.GetComponentPool(componentType).GetInstance(this,entity,out uint index);
            uint componentTypeIndex = componentType.GetIndex();
            entitySearchSparseArrays[componentTypeIndex].SetIndex(component.ComponentID,entity.EntityID);
            componentSearchSparseArrays[componentTypeIndex].SetIndex(entity.EntityID,component.ComponentID);
            entityManager.AddComponentMask(entity.EntityID,componentType.ToMask());
            return true;
        }

        public bool AddComponent(Entity entity,ComponentTypeEnum componentType) {
            if(entity.HasComponent(componentType))
                return true;
            var component = componentPoolManager.GetComponentPool(componentType).GetInstance(this,entity,out uint index);
            uint componentTypeIndex = componentType.GetIndex();
            entitySearchSparseArrays[componentTypeIndex].SetIndex(component.ComponentID,entity.EntityID);
            componentSearchSparseArrays[componentTypeIndex].SetIndex(entity.EntityID,component.ComponentID);
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

        #region RemoveComponent (ֵ����ʵ��)
        public bool RemoveComponent(Entity entity,ComponentTypeEnum componentType) {
            if(!entity.HasComponent(componentType)) {
                Debug.LogError($"Entity:{entity.EntityID} doesn't has this type of Component:{componentType}");
                return false;
            }
            uint componentTypeIndex = componentType.GetIndex();
            uint supposeComponentID = componentSearchSparseArrays[componentTypeIndex].GetIndex(entity.EntityID);
            var component = componentPoolManager.GetComponentPool(componentType).GetActiveInstance(supposeComponentID);
            if(component == null) {
                Debug.LogError($"Didn't get active instance of:{componentType} on the entity:{entity}");
                return false;
            }
            if(supposeComponentID != component.ComponentID) {
                Debug.LogError($"componentID dismatch ,we found:{supposeComponentID},actually:{component.ComponentID}");
                return false;
            }

            var supposeEntity = entityManager.GetEntityCopy(entitySearchSparseArrays[componentTypeIndex].GetIndex(component.ComponentID));
            if(supposeEntity != entity) {
                Debug.LogError($"Entity dismatch,wo found:{supposeEntity},actually:{entity}");
                return false;
            }

            componentPoolManager.GetComponentPool(componentType).ReleaseInstance(this,component,entity);
            componentSearchSparseArrays[componentTypeIndex].RemoveIndex(entity.EntityID);
            entitySearchSparseArrays[componentTypeIndex].RemoveIndex(component.ComponentID);
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

        public TSystem GetSystemByType<TSystem>() where TSystem : ISystem {
            Type type = typeof(TSystem);
            foreach(var sys in systems) {
                if(sys.GetType() == type) {
                    return (TSystem)sys;
                }
            }
            return default;
        }
        #endregion

        #region Life Time
        public void OnUpdate(int localFrameCount,float deltaTime) {
            foreach(var sys in systems) {
                sys.OnFrameUpdate(this,localFrameCount,deltaTime);
            }
        }

        public void OnLateUpdate(int localFrameCount,float deltaTime) {
            foreach(var sys in systems) {
                sys.OnFrameLateUpdate(this,localFrameCount);
            }

            foreach(var query in activeQuriesCurrentFrame) {
                ReferencePoolingCenter.Instance.ReleaseReference(query);
            }
            activeQuriesCurrentFrame.Clear();
        }

        public void OnNetworkUpdate(int networkFrameCount) {
            foreach(var sys in systems) {
                sys.OnNetworkUpdate(this,networkFrameCount);
            }
        }

        public void OnRollingBack(int errorStartFrameCount,int currentSimulateFrameCount,float deltaTime) {
            foreach(var sys in systems) {
                if(sys.GetType() == typeof(InputSystem)) {
                    (sys as InputSystem).OnRollingBackState(this);
                } else {
                    sys.OnFrameUpdate(this,currentSimulateFrameCount,deltaTime);
                }
            }

            foreach(var sys in systems) {
                if(sys.GetType() != typeof(InputSystem)) {
                    sys.OnFrameLateUpdate(this,currentSimulateFrameCount);
                }
            }
        }

        public void OnDestroy() {
            foreach(var sys in systems) {
                sys.OnDestroy(this);
            }
            systems.Clear();
            systems = null;

            entityManager.OnDestroy();
            componentPoolManager.OnDestroy();
            registration.OnDestroy();
            foreach(var sparseArray in entitySearchSparseArrays) {
                sparseArray.OnDestroy();
            }
            foreach(var sparseArray in componentSearchSparseArrays) {
                sparseArray.OnDestroy();
            }
            entitySearchSparseArrays = null;
            componentSearchSparseArrays = null;
            foreach(var query in activeQuriesCurrentFrame) {
                ReferencePoolingCenter.Instance.ReleaseReference(query);
            }
            activeQuriesCurrentFrame.Clear();
            activeQuriesCurrentFrame = null;
        }
        #endregion

        public World() {
            registration = new GameObjectRegistration();
            componentPoolManager = new ComponentPoolManager();
            entityManager = new EntityManager();
            entitySearchSparseArrays = new SparseArray[ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT];
            componentSearchSparseArrays = new SparseArray[ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT];
            activeQuriesCurrentFrame = new List<Query>();

            for(int i = 0; i < ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT; i++) {
                entitySearchSparseArrays[i] = new SparseArray();
                componentSearchSparseArrays[i] = new SparseArray();
            }

            LoadAllSystems();
        }

        void LoadAllSystems() {
            systems = new List<ISystem>();
            ISystem system;
            foreach(var type in SystemTypeCollection.SystemTypes) {
                system = (ISystem)System.Activator.CreateInstance(type);
                systems.Add(system);
                if(system.GetType() == typeof(InputSystem)) {
                    inputSystem = system as InputSystem;
                }
            }

            systems.Sort((a,b) => a.Order < b.Order ? -1 : 1);
            foreach(var _system in systems) {
                _system.OnInit(this);
            }
        }
    }
}