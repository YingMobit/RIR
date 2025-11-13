using GAS;
using System;

namespace PoolingSystem.ReferencePool {
    public class ReferenceTypes {
        public const uint COMPONENT_SET = 0;
        public const uint QUERY = 1;
        public const uint CHARACTORTRANSFORMCONTROLLER = 2;
        public const uint CHARACTORANIMATIONCONTROLLER = 3;
        public const uint GAMEOBJECTPOOL = 4;
        public const uint ABILITYEXCUTIONTASK = 5;
        public const uint ABILITYRUNTIMECONTEXT = 6;
        public const uint BLACKBOARD = 7;
        public const uint ABILITYCOMPONENT = 8;
        public const uint ABILITYCOMPONENTCONTEXT = 9;
        public const uint ATTRIBUTESET = 10;
        public const uint ATTRIBUTE = 11;

        public const int TYPE_COUNT = 12;

        private static Type[] types = new Type[TYPE_COUNT] {
            typeof(ECS.ComponentSet), // index 0
            typeof(ECS.Query), // index 1
            typeof(CharactorTransformController),
            typeof(CharactorAnimationController),
            typeof(GameObjectPool.GameObjectPool),
            typeof(AbilityExcutionTask),
            typeof(AbilityRuntimeContext),
            typeof(BlackBoard),
            typeof(AbilityComponent),
            typeof(AbilityComponentContext),
            typeof(AttributeSet),
            typeof(GAS.Attribute)
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