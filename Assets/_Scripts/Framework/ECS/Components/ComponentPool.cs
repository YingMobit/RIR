using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ECS {
    /// <summary>
    /// 为了保证稀疏数组有无效值，0号组件为无效组件，组件池永远不会租出它
    /// </summary>
    public sealed class ComponentPool {

        public const int DEFAULT_BUCKET_CAPACITY = 64;
        public int ActiveComponentCount => components.Count - freeComponentIndexs.Count - 1;
        public int FreeComponentCount => freeComponentIndexs.Count;
        public int TotalComponentCount => components.Count;
        private ComponentTypeEnum componentTypeEnum;
        private List<Component> components;
        private bool[] activeMap;
        private Stack<uint> freeComponentIndexs;
        private Component componentTemplate;


        public void Init(ComponentTypeEnum componentTypeEnum) {
            this.componentTypeEnum = componentTypeEnum;
            components = new(DEFAULT_BUCKET_CAPACITY);
            freeComponentIndexs = new(DEFAULT_BUCKET_CAPACITY);
            activeMap = new bool[DEFAULT_BUCKET_CAPACITY];
            componentTemplate = (Component)Activator.CreateInstance(ComponentTypeEnumExtension.COMPONENT_TYPE_MAPPING[componentTypeEnum.GetIndex()]);
            components.Add(componentTemplate.SetComponentID(0));
            for(uint i = 1; i < DEFAULT_BUCKET_CAPACITY; i++) {
                components.Add(componentTemplate.Clone().SetComponentID(i));
                freeComponentIndexs.Push(i);
            }
        }

        private void ExpandPool() {
            uint startIndex = (uint)TotalComponentCount;
            components.Capacity += DEFAULT_BUCKET_CAPACITY;
            Array.Resize(ref activeMap,components.Capacity);
            for(uint i = 0; i < DEFAULT_BUCKET_CAPACITY; i++) {
                if(startIndex + i != 0) {
                    components.Add(componentTemplate.Clone().SetComponentID(startIndex + i));
                    freeComponentIndexs.Push(startIndex + i);
                }
            }
        }

        /// <summary>
        /// 申请一个组件实例并绑定实体。返回该实例，输出槽位索引。
        /// </summary>
        public Component GetInstance(Entity entity,out uint index) {
            if(freeComponentIndexs.Count == 0) {
                ExpandPool();
            }
            index = freeComponentIndexs.Pop();
            components[(int)index].OnAttach(entity);
            activeMap[index] = true;
            return components[(int)index];
        }

        /// <summary>
        /// 归还组件实例。若组件不属于该池将抛异常。
        /// </summary>
        public void ReleaseInstance(Component component,Entity entity,uint index) {
            if(index >= TotalComponentCount) {
                Debug.LogError($"index out of range:{index}");
                return;
            }
            if(index == 0) {
                Debug.LogError("Zero index Component should not be rented");
                return;
            }
            if(component.ComponentType != componentTypeEnum) {
                Debug.LogError($"Component Type dismatch,wanted:{componentTypeEnum}, actual: {component.ComponentType}");
                return;
            }
            if(component != GetActiveInstance(index)) {
                Debug.LogError("Not the same Component");
            }
            component.Reset(entity);
            freeComponentIndexs.Push(index);
            activeMap[index] = false;
        }

        /// <summary>
        /// 根据槽位索引取得活跃组件实例；如果非活跃返回 null。
        /// </summary>
        public Component GetActiveInstance(uint index) {
            if(index == 0) {
                Debug.LogError("Zero index Component should not be rented");
                return null;
            }
            if(!activeMap[index]) {
                Debug.LogError($"this component is not active:{index}");
                return null;
            }
            return components[(int)index];
        }

        /// <summary>
        /// 返回当前所有活跃组件（复制列表）。
        /// </summary>
        public List<Component> GetAllActiveComponents() {
            List<Component> res = new(ActiveComponentCount);
            int maxCount = TotalComponentCount;
            for(int i = 1; i < maxCount; i++) {
                if(activeMap[i]) {
                    res.Add(components[i]);
                }
            }
            return res;
        }
    }
}