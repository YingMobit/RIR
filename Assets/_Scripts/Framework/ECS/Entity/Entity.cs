using System;

namespace ECS {
    public struct Entity {
        public uint EntityID { get; private set; }
        public int GameObjectID { get; private set; }
        public short Version { get; private set; }
        public uint Archetype { get; private set; }

        public readonly bool HasComponent(ComponentTypeEnum componentTypeMask) {
            uint mask = componentTypeMask.ToMask();
            return (Archetype & mask) == mask;
        }

        public readonly bool HasAllComponents(uint componentTypeMask) {
            return (Archetype & componentTypeMask) == componentTypeMask;
        }

        public readonly bool HasAnyComponent(uint componentTypeMask) {
            return (Archetype & componentTypeMask) != 0;
        }

        public readonly bool WithOutComponent(ComponentTypeEnum componentTypeMask) {
            uint mask = componentTypeMask.ToMask();
            return (Archetype & mask) == 0;
        }

        public readonly bool WithOutAllComponents(uint componentTypeMask) {
            return (Archetype & componentTypeMask) == 0;
        }

        public readonly bool WithOutAnyComponent(uint componentTypeMask) {
            return (Archetype & componentTypeMask) != 0;
        }

        internal void OnAddComponent(uint componentTypesToAdd) {
            Archetype |= componentTypesToAdd;
        }

        internal void OnRemoveComponent(uint componentTypesToRemove) {
            Archetype &= ~componentTypesToRemove;
        }

        public void Set(uint entityID,int gameObjectID,short version,uint archeType) {
            EntityID = entityID;
            GameObjectID = gameObjectID;
            Version = version;
            Archetype = archeType;
        }

        public void SetArchetype(uint archeType) {
            Archetype = archeType;
        }

        public Entity(uint entityID,int gameObjectID,short version,uint archeType) {
            EntityID = entityID;
            GameObjectID = gameObjectID;
            Version = version;
            Archetype = archeType;
        }

        public static bool operator ==(Entity a,Entity b) {
            return a.EntityID == b.EntityID && a.Version == b.Version;
        }

        public static bool operator !=(Entity a,Entity b) {
            return !(a == b);
        }

        public override int GetHashCode() {
            return (int)(EntityID << 16) | (ushort)Version;
        }
    }
}
