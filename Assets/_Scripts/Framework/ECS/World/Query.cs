using System;
using System.Collections.Generic;
using ReferencePoolingSystem;
using UnityEngine.Pool;

namespace ECS {
    public class Query : IReference<Query> {
        World world;
        List<ComponentSet> componentSets;
        List<Entity> entities;
        uint includeMask;
        uint excludeMask;

        public IReadOnlyList<ComponentSet> ComponentSets => componentSets;
        public IReadOnlyList<Entity> Entities => entities;

        public Query With(ComponentTypeEnum componentType) {
            uint mask = componentType.ToMask();
            if((includeMask & mask) == mask)
                return this;
            includeMask |= mask;
            List<Component> components = ListPool<Component>.Get();
            if(entities.Count == 0 && componentSets.Count == 0) {
                world.GetComponents(componentType,in components,in entities);
                foreach(var comp in components) {
                    componentSets.Add(ReferencePoolingCenter.Instance.GetReference<ComponentSet>().AddComponent(componentType,comp));
                }
            } else {
                Fliter();
                world.GetComponentsOnEntities(entities,componentType,in components);
                for(int i = 0; i < componentSets.Count; i++) {
                    componentSets[i].AddComponent(componentType,components[i]);
                }
            }
            ListPool<Component>.Release(components);
            return this;
        }

        public Query Without<TComponent>(ComponentTypeEnum componentType) where TComponent : Component {
            excludeMask |= componentType.ToMask();
            if(entities.Count == 0 && componentSets.Count == 0) {
                return this;
            } else {
                Fliter();
            }
            return this;
        }

        private void Fliter() {
            for(int i = entities.Count - 1; i >= 0; i--) {
                if(!entities[i].HasAllComponents(includeMask) || entities[i].HasAnyComponent(excludeMask)) {
                    entities.RemoveAt(i);
                    var set = componentSets[i];
                    componentSets.RemoveAt(i);
                    ReferencePoolingCenter.Instance.ReleaseReference(set);
                }
            }
        }


        public Query() {
            componentSets = new List<ComponentSet>();
            entities = new List<Entity>();
            includeMask = 0;
            excludeMask = 0;
        }

        public Query BindWorld(World world) {
            this.world = world;
            return this;
        }
        int IReference.IndexInRefrencePool { get; set; }
        public void OnRecycle() {
            foreach(var set in componentSets) {
                ReferencePoolingCenter.Instance.ReleaseReference(set);
            }
            componentSets.Clear();
            entities.Clear();
            includeMask = 0;
            excludeMask = 0;
        }

        public void Dispose() {
            OnRecycle();
            componentSets = null;
            entities = null;
        }

        public IReference Clone() {
            return new Query();
        }
    }
}