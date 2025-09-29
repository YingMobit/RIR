using System;
using System.Collections.Generic;
using ReferencePoolingSystem;
using UnityEngine;

namespace ECS {
    public class ComponentSet : IReference<ComponentSet> {
        public uint ReferenceType => ReferenceTypes.COMPONENT_SET;
        private Component[] components = new Component[ComponentTypeEnumExtension.COMPONENT_TYPE_COUNT];

        public TComponent GetComponent<TComponent>(ComponentTypeEnum componentType) where TComponent : Component {
            uint index = componentType.GetIndex();
            if(components[index] != null) {
                return components[index] as TComponent;
            }
            return null;
        }

        public Component GetComponent(ComponentTypeEnum componentType) {
            uint index = componentType.GetIndex();
            if(components[index] != null) {
                return components[index];
            }
            return null;
        }

        public ComponentSet AddComponent(ComponentTypeEnum componentType,Component component) {
            uint index = componentType.GetIndex();
            if(components[index] != null) {
                Debug.LogError($"ComponentSet Already Contains Component of Type {componentType}");
            }
            components[index] = component;
            return this;
        }

        public void OnRecycle() {
            Array.Clear(components, 0, components.Length);
        }
        public void Dispose() {
            OnRecycle();
            Array.Clear(components, 0, components.Length);
            components = null;
        }

        public IReference Clone() {
            return new ComponentSet();
        }

        int IReference.IndexInRefrencePool { get; set; }
    }
}