using InputSystemNameSpace;
using System;
using System.Collections.Generic;

namespace ECS { 
    public static class SystemTypeCollection {
        public static List<Type> SystemTypes = new() {
            typeof(InputSystem),
            typeof(AbilitySystem),
        }; 
    }
}