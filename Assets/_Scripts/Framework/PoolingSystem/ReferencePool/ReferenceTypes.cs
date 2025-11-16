using System;
using GAS;
using RollBackSystem;

namespace PoolingSystem.ReferencePool {
    public class ReferenceTypes {
        public const uint COMPONENT_SET = 0;
        public const uint QUERY = 1;
        public const uint GAMEOBJECTPOOL = 2;
        public const uint ABILITYEXCUTIONTASK = 3;
        public const uint ABILITYRUNTIMECONTEXT = 4;
        public const uint BLACKBOARD = 5;
        public const uint ABILITYCOMPONENT = 6;
        public const uint ABILITYCOMPONENTCONTEXT = 7;
        public const uint ATTRIBUTESET = 8;
        public const uint ATTRIBUTE = 9;
        public const uint SNAPSHOTHANDLER = 10;
        public const int TYPE_COUNT = 11;

        private static Type[] types = new Type[TYPE_COUNT] {
            typeof(ECS.ComponentSet), // index 0
            typeof(ECS.Query), // index 1
            typeof(GameObjectPool.GameObjectPool),
            typeof(AbilityExcutionTask),
            typeof(AbilityRuntimeContext),
            typeof(BlackBoard),
            typeof(AbilityComponent),
            typeof(AbilityComponentContext),
            typeof(AttributeSet),
            typeof(GAS.Attribute),
            typeof(SnapShotHandler)
    };
        private static Type tempType;

        public static int GetReferenceTypeIndex<TReference>() where TReference : IReference<TReference>, new() {
            tempType = typeof(TReference);
            for(int i = 0; i < TYPE_COUNT; i++) {
                if(types[i] == tempType)
                    return i;
            }
            return -1;
        }
    }
}