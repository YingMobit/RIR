using System.Collections.Generic;
using System;
using ReferencePoolingSystem;

using UnityEngine;

namespace ECS { 
    public class ComponentSet : IReference<ComponentSet> {
        private Dictionary<ComponentTypeEnum,Component> components = new();

        public TComponent GetComponent<TComponent>(ComponentTypeEnum componentType) where TComponent : Component {
            if(components.ContainsKey(componentType)) {
                return components[componentType] as TComponent;
            }
            return null;
        }

        public ComponentSet AddComponent(ComponentTypeEnum componentType,Component component) {
            if(components.ContainsKey(componentType)) {
                Debug.LogError($"ComponentSet Already Contains Component of Type {componentType}");
            }
            components[componentType] = component;
            return this;
        }

        public void OnRecycle() {
            components.Clear();
        }

        public void Dispose() {
            OnRecycle();
            components = null;
        }
    }
}