using System.Collections.Generic;
using System;
using ReferencePoolingSystem;

namespace ECS { 
    public class ComponentSet : IReference<ComponentSet> {
        private Dictionary<Type,Component> components = new();

        public TComponent GetComponent<TComponent>() where TComponent : Component {
            Type type = typeof(TComponent);
            if(components.ContainsKey(type)) {
                return components[type] as TComponent;
            }
            return null;
        }

        public ComponentSet AddComponent<TComponent>(TComponent component) where TComponent : Component {
            Type type = typeof(TComponent);
            if(components.ContainsKey(type)) {
                throw new ArgumentException($"ComponentSet Already Contains Component of Type {type}");
            }
            components[type] = component;
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