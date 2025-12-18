using InputSystemNameSpace;
using System;
using System.Collections.Generic;

namespace ECS { 
    public static class SystemTypeCollection {
        public static List<Type> SystemTypes = new() {
            typeof(BulletSystem),
            typeof(RollBackSystem.RollBackSystem),
            typeof(InputSystem),
            typeof(AbilitySystem),
        }; 
    }
}