using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    internal class GameObjectRegistration {
        private Dictionary<int,GameObject> IDGameObjectMap = new();

        public int GetID(GameObject gameObject) {
            if(gameObject) {
                int id = gameObject.GetInstanceID();
                if(!IDGameObjectMap.ContainsKey(id)) {
                    IDGameObjectMap.Add(id,gameObject);
                }
                return id;
            }
            return -1;
        }

        public void OnReleaseEntity(Entity entity) {
            if(IDGameObjectMap.ContainsKey(entity.GameObjectID)) {
                //IDGameObjectMap.Remove(entity.GameObjectID);
            } else {
                Debug.LogError($"Haven't registed this ID:{entity.GameObjectID} befor");
            }
        }
    }
}
