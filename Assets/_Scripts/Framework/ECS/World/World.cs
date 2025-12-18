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
    public class World {
        private GameObjectRegistration _registration;
        private ComponentPoolManager _componentPoolManager;
        private EntityManager _entityManager;
        private SparseArray[] _entitySearchSparseArrays;
        private SparseArray[] _componentSearchSparseArrays;
        private List<Query> _activeQuriesCurrentFrame;

        private List<ISystem> _systems;
        private InputSystem _inputSystem;

        #region API
        public int GetEntityCount() => (int)_entityManager.TotalEntityCount;
        public int GetActiveEntityCount() => (int)_entityManager.ActiveEntityCount;
        public int GetComponentCount(ComponentTypeEnum componentType) => _componentPoolManager.GetComponentPool(componentType).TotalComponentCount;
        public int GetActiveComponentCount(ComponentTypeEnum componentType) => _componentPoolManager.GetComponentPool(componentType).ActiveComponentCount;

        public Entity GetEntity(GameObject gameObject,uint componentTypeMask) {
            Entity newEntity = _entityManager.GetEntity(_registration.GetID(gameObject));
            if(componentTypeMask != 0)
                AddComponents(newEntity,componentTypeMask);
            return _entityManager.GetEntityCopy(newEntity.EntityID);
        }
        
        public Entity GetLatestEntity(uint entityID) => _entityManager.GetEntityCopy(entityID);

        public Entity GetLatestEntity(Entity entity) {
            return _entityManager.GetEntityCopy(entity.EntityID);
        }

        public GameObject GetGameObject(Entity entity) {
            return _registration.GetGameObject(entity.GameObjectID);
        }

        public void ReleaseEntity(Entity entity) {
            entity = GetLatestEntity(entity);
            RemoveAllComponents(entity);
            _registration.OnReleaseEntity(entity);
            _entityManager.ReleaseEntity(entity);
        }


        #region GetComponents
        public void GetComponents(ComponentTypeEnum componentType,in List<Component> components) {
            _componentPoolManager.GetComponentPool(componentType).GetAllActiveComponents(components);
        }

        public void GetComponents(ComponentTypeEnum componentType,in List<Component> components,in List<Entity> entityCopies) {
            _componentPoolManager.GetComponentPool(componentType).GetAllActiveComponents(components);
            int count = components.Count;
            if(entityCopies.Capacity < count)
                entityCopies.Capacity = count;
            entityCopies.Clear();
            for(int i = 0; i < count; i++) {
                entityCopies.Add(_entityManager.GetEntityCopy(_entitySearchSparseArrays[componentType.GetIndex()].GetIndex(components[i].ComponentID)));
            }
        }
        public void GetComponentOnEntity(Entity entity,ComponentTypeEnum componentType,out Component component) {
            entity = GetLatestEntity(entity);
            component = _componentPoolManager.GetComponentPool(componentType).GetActiveInstance(_componentSearchSparseArrays[componentType.GetIndex()].GetIndex(entity.EntityID));
        }

        public void GetAllComponentsOnEntity(Entity entity,in List<Component> components) {
            entity = GetLatestEntity(entity);
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
            _activeQuriesCurrentFrame.Add(query);
            return query;
        }
        #endregion

        #region AddComponent
        public bool AddComponent(Entity entity,ComponentTypeEnum componentType,out Component component) {
            entity = GetLatestEntity(entity);
            if(entity.HasComponent(componentType)) {
                GetComponentOnEntity(entity,componentType,out component);
                return true;
            }
            component = _componentPoolManager.GetComponentPool(componentType).GetInstance(this,entity,out uint index);
            uint componentTypeIndex = componentType.GetIndex();
            _entitySearchSparseArrays[componentTypeIndex].SetIndex(component.ComponentID,entity.EntityID);
            _componentSearchSparseArrays[componentTypeIndex].SetIndex(entity.EntityID,component.ComponentID);
            _entityManager.AddComponentMask(entity.EntityID,componentType.ToMask());
            return true;
        }

        public bool AddComponent(Entity entity,ComponentTypeEnum componentType) {
            entity = GetLatestEntity(entity);
            if(entity.HasComponent(componentType))
                return true;
            var component = _componentPoolManager.GetComponentPool(componentType).GetInstance(this,entity,out uint index);
            uint componentTypeIndex = componentType.GetIndex();
            _entitySearchSparseArrays[componentTypeIndex].SetIndex(component.ComponentID,entity.EntityID);
            _componentSearchSparseArrays[componentTypeIndex].SetIndex(entity.EntityID,component.ComponentID);
            _entityManager.AddComponentMask(entity.EntityID,componentType.ToMask());
            return true;
        }

        public bool AddComponents(Entity entity,uint componentTypeMask) {
            entity = GetLatestEntity(entity);
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

        #region RemoveComponent
        public bool RemoveComponent(Entity entity,ComponentTypeEnum componentType) {
            entity = GetLatestEntity(entity);
            if(!entity.HasComponent(componentType)) {
                Debug.LogError($"Entity:{entity.EntityID} doesn't has this type of Component:{componentType}");
                return false;
            }
            uint componentTypeIndex = componentType.GetIndex();
            uint supposeComponentID = _componentSearchSparseArrays[componentTypeIndex].GetIndex(entity.EntityID);
            var component = _componentPoolManager.GetComponentPool(componentType).GetActiveInstance(supposeComponentID);
            if(component == null) {
                Debug.LogError($"Didn't get active instance of:{componentType} on the entity:{entity}");
                return false;
            }
            if(supposeComponentID != component.ComponentID) {
                Debug.LogError($"componentID dismatch ,we found:{supposeComponentID},actually:{component.ComponentID}");
                return false;
            }

            var supposeEntity = _entityManager.GetEntityCopy(_entitySearchSparseArrays[componentTypeIndex].GetIndex(component.ComponentID));
            if(supposeEntity != entity) {
                Debug.LogError($"Entity dismatch,wo found:{supposeEntity},actually:{entity}");
                return false;
            }

            _componentPoolManager.GetComponentPool(componentType).ReleaseInstance(this,component,entity);
            _componentSearchSparseArrays[componentTypeIndex].RemoveIndex(entity.EntityID);
            _entitySearchSparseArrays[componentTypeIndex].RemoveIndex(component.ComponentID);
            _entityManager.RemoveComponentMask(entity.EntityID,componentType.ToMask());
            return true;
        }

        public bool RemoveComponents(Entity entity,uint componentTypeMask) {
            entity = GetLatestEntity(entity);
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
            entity = GetLatestEntity(entity);
            uint componentMask = entity.Archetype;
            if(componentMask == 0)
                return false;
            return RemoveComponents(entity,componentMask);
        }
        #endregion

        public TSystem GetSystemByType<TSystem>() where TSystem : ISystem {
            Type type = typeof(TSystem);
            foreach(var sys in _systems) {
                if(sys.GetType() == type) {
                    return (TSystem)sys;
                }
            }
            return default;
        }
        #endregion

        #region Life Time
        public void OnUpdate(int localFrameCount,float deltaTime) {
            foreach(var sys in _systems) {
                sys.OnFrameUpdate(this,localFrameCount,deltaTime);
            }
        }

        public void OnLateUpdate(int localFrameCount,float deltaTime) {
            foreach(var sys in _systems) {
                sys.OnFrameLateUpdate(this,localFrameCount);
            }

            foreach(var query in _activeQuriesCurrentFrame) {
                ReferencePoolingCenter.Instance.ReleaseReference(query);
            }
            _activeQuriesCurrentFrame.Clear();
        }

        public void OnNetworkUpdate(int networkFrameCount) {
            foreach(var sys in _systems) {
                sys.OnNetworkUpdate(this,networkFrameCount);
            }
        }

        public void OnRollingBack(int errorStartFrameCount,int currentSimulateFrameCount,float deltaTime) {
            foreach(var sys in _systems) {
                if(sys.GetType() == typeof(InputSystem)) {
                    (sys as InputSystem).OnRollingBackState(this);
                } else {
                    sys.OnFrameUpdate(this,currentSimulateFrameCount,deltaTime);
                }
            }

            foreach(var sys in _systems) {
                if(sys.GetType() != typeof(InputSystem)) {
                    sys.OnFrameLateUpdate(this,currentSimulateFrameCount);
                }
            }
        }

        public void OnDestroy() {
            foreach(var sys in _systems) {
                sys.OnDestroy(this);
            }
            _systems.Clear();
            _systems = null;

            _entityManager.OnDestroy();
            _componentPoolManager.OnDestroy();
            _registration.OnDestroy();
            foreach(var sparseArray in _entitySearchSparseArrays) {
                sparseArray.OnDestroy();
            }
            foreach(var sparseArray in _componentSearchSparseArrays) {
                sparseArray.OnDestroy();
            }
            _entitySearchSparseArrays = null;
            _componentSearchSparseArrays = null;
            foreach(var query in _activeQuriesCurrentFrame) {
                ReferencePoolingCenter.Instance.ReleaseReference(query);
            }
            _activeQuriesCurrentFrame.Clear();
            _activeQuriesCurrentFrame = null;
        }
        #endregion

        public World() {
            _registration = new GameObjectRegistration();
            _componentPoolManager = new ComponentPoolManager();
            _entityManager = new EntityManager();
            _entitySearchSparseArrays = new SparseArray[ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT];
            _componentSearchSparseArrays = new SparseArray[ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT];
            _activeQuriesCurrentFrame = new List<Query>();

            for(int i = 0; i < ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT; i++) {
                _entitySearchSparseArrays[i] = new SparseArray();
                _componentSearchSparseArrays[i] = new SparseArray();
            }

            LoadAllSystems();
        }

        void LoadAllSystems() {
            _systems = new List<ISystem>();
            ISystem system;
            foreach(var type in SystemTypeCollection.SystemTypes) {
                system = (ISystem)System.Activator.CreateInstance(type);
                _systems.Add(system);
                if(system.GetType() == typeof(InputSystem)) {
                    _inputSystem = system as InputSystem;
                }
            }

            _systems.Sort((a,b) => a.Order < b.Order ? -1 : 1);
            foreach(var _system in _systems) {
                _system.OnInit(this);
            }
        }
    }
}