using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ECS{
    public class World {
        private GameObjectRegistration registration;


        #region API
        public Entity GetEntity(GameObject gameObject) {
            return default;
        }

        public List<Component> GetComponents(ComponentTypeEnum componentTypeMask) {
            return default;
        }

        public List<Component> GetComponents(ComponentTypeEnum componentTypeMask,out List<Entity> entities) {
            entities = default;
            return default;
        }
        #endregion
    }
}
