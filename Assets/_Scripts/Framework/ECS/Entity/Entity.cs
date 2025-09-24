namespace ECS {
    public struct Entity {
        public int EntityID;
        public int GameObjectID;
        public short Version;
        public long Anchetype;

        public Entity(int entityID,int gameObjectID,short version,long ancheType) { 
            EntityID = entityID;
            GameObjectID = gameObjectID;
            Version = version;
            Anchetype = ancheType;
        }
    }
}
