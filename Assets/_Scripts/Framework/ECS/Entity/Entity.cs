using System;

namespace ECS {
    public struct Entity : IEquatable<Entity> {
        public int EntityID { get; private set; }
        public int GameObjectID { get; private set; }
        public short Version { get; private set; }
        public uint Anchetype { get; private set; }

        public readonly bool HasComponent(ComponentTypeEnum componentTypeMask) {
            uint mask = componentTypeMask.ToMask();
            return (Anchetype & mask) == mask;
        }

        public readonly bool HasAllComponents(uint componentTypeMask) {
            return (Anchetype & componentTypeMask) == componentTypeMask;
        }

        public readonly bool HasAnyComponent(uint componentTypeMask) {
            return (Anchetype & componentTypeMask) != 0;
        }

        public readonly bool WithOutComponent(ComponentTypeEnum componentTypeMask) {
            uint mask = componentTypeMask.ToMask();
            return (Anchetype & mask) == 0;
        }

        public readonly bool WithOutAllComponents(uint componentTypeMask) {
            return (Anchetype & componentTypeMask) == 0;
        }

        public readonly bool WithOutAnyComponent(uint componentTypeMask) {
            return (Anchetype & componentTypeMask) != 0;
        }

        internal void OnAddComponent(uint componentTypes) { 
            Anchetype |= componentTypes;
        }

        internal void OnRemoveComponent(uint componentTypes) { 
            Anchetype &= ~componentTypes;
        }

        public void Set(int entityID,int gameObjectID,short version,uint ancheType) {
            EntityID = entityID;
            GameObjectID = gameObjectID;
            Version = version;
            Anchetype = ancheType;
        }

        public Entity(int entityID,int gameObjectID,short version,uint ancheType) {
            EntityID = entityID;
            GameObjectID = gameObjectID;
            Version = version;
            Anchetype = ancheType;
        }

        public static bool operator == (Entity a,Entity b) {
            return a.EntityID == b.EntityID && a.Version == b.Version;
        }

        public static bool operator != (Entity a,Entity b) {
            return !(a == b);
        }

        public override int GetHashCode() {
            return (EntityID << 16) | (ushort)Version;
        }

        bool IEquatable<Entity>.Equals(Entity other) {
            return this == other;
        }
    }
}
