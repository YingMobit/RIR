using ReferencePoolingSystem;
using System.Collections.Generic;

namespace ECS { 
    public class Query : IReference<Query> {
        World world;
        List<ComponentSet> componentSets;
        List<Entity> entities;
        uint includeMask;
        uint excludeMask;

        public IReadOnlyList<ComponentSet> ComponentSets => componentSets;
        public IReadOnlyList<Entity> Entities => entities;

        public Query With<TComponent>(ComponentTypeEnum componentType) where TComponent : Component , new() {
            uint mask = componentType.ToMask();
            if((includeMask & mask) == mask)
                return this;
            includeMask |= mask;
            if(entities.Count == 0 && componentSets.Count == 0) {
                List<TComponent> components = world.GetComponents<TComponent>(componentType,out entities);
                foreach(var comp in components) { 
                    componentSets.Add(ReferencePoolingCenter.Instance.GetReference<ComponentSet>().AddComponent(comp));
                }
            } else {
                Fliter();
                var newComponents = world.GetComponentsOnEntities<TComponent>(entities,componentType);
                for(int i = 0; i < componentSets.Count; i++) {
                    componentSets[i].AddComponent(newComponents[i]);
                }
            }
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
            for(int i = entities.Count-1;i >=0 ; i--) {
                if(!entities[i].HasAllComponents(includeMask) || entities[i].HasAnyComponent(excludeMask)) { 
                    entities.RemoveAt(i);
                    componentSets.RemoveAt(i);
                }
            }
        }

        public void OnRecycle() {
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
    }
}