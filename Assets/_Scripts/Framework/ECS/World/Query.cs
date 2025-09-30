using System;
using System.Collections.Generic;
using System.Linq;
using ReferencePoolingSystem;
using UnityEngine.Pool;

namespace ECS {
    public class Query : IReference<Query> {
        World world;
        List<ComponentSet> componentSets;
        List<Entity> entities;
        ComponentTypeEnum[] componentTypes;
        uint includeMask;
        uint excludeMask;

        public IReadOnlyList<ComponentSet> ComponentSets => componentSets;
        public IReadOnlyList<Entity> Entities => entities;

        public Query With(ComponentTypeEnum componentType) {
            includeMask |= componentType.ToMask();
            return this;
        }

        public Query Without<TComponent>(ComponentTypeEnum componentType) where TComponent : Component {
            excludeMask |= componentType.ToMask();
            return this;
        }

        public Query Execute() {
            componentTypes = includeMask.MaskToEnums();
            if(componentTypes.Length == 0) return this;

            int minCount = int.MaxValue;
            int count;
            ComponentTypeEnum pivotType = componentTypes[0];
            foreach(var type in componentTypes) {
                count = world.GetActiveComponentCount(type);
                if(count < minCount) { 
                    pivotType = type;
                    minCount = count;
                }
            }

            List<Component> pivotComponents = ListPool<Component>.Get();
            world.GetComponents(pivotType,pivotComponents,entities);
            for(int i= entities.Count - 1; i >= 0; i--) {
                if(!(entities[i].HasAllComponents(includeMask) && entities[i].WithOutAllComponents(excludeMask))) {
                    if(i == entities.Count - 1) {
                        pivotComponents.RemoveAt(i);
                        entities.RemoveAt(i);
                    } else {
                        pivotComponents[i] = pivotComponents[pivotComponents.Count - 1];
                        pivotComponents.RemoveAt(pivotComponents.Count - 1);
                        entities[i] = entities[entities.Count - 1];
                        entities.RemoveAt(entities.Count - 1);
                    }
                }
            }

            int fitCount = entities.Count;
            ComponentSet set;
            Component temp;
            for(int i=0;i < fitCount; i++) {
                set = world.ReferencePoolingCenter.GetReference<ComponentSet>();
                componentSets.Add(set);
                set.AddComponent(pivotType,pivotComponents[i]);
                for(int j = 0;j < componentTypes.Length ; j++) {
                    if(componentTypes[j] != pivotType) {
                        world.GetComponentOnEntity(entities[i],componentTypes[j],out temp);
                        set.AddComponent(componentTypes[j],temp);
                    }
                }
            }

            set = null;
            temp = null;
            pivotComponents.Clear();
            ListPool<Component>.Release(pivotComponents);
            return this;
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

        public uint ReferenceType => ReferenceTypes.QUERY;
        int IReference.IndexInRefrencePool { get; set; }
        public void OnRecycle() {
            foreach(var set in componentSets) {
                world.ReferencePoolingCenter.ReleaseReference(set);
            }
            componentSets.Clear();
            entities.Clear();
            if(componentTypes != null) Array.Clear(componentTypes,0,componentTypes.Length);
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