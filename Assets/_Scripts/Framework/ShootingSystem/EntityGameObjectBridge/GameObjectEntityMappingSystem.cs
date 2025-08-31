using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Utility;

public class GameObjectEntityMappingSystem : Singleton<GameObjectEntityMappingSystem> {
    private Dictionary<GameObject,int> GameObject2EntityMap = new();
    private Dictionary<int,GameObject> Entity2GameObjectMap = new();

    public void Regist(int entityID,GameObject gameObject) {
        if(!gameObject) {
            Debug.LogError("GameObject is null");
            return;
        }
        if(entityID <= 0) {
            Debug.LogError($"Entity is null or inValid,entityID: {entityID}");
            return;
        }
        var go2EntityExist = GameObject2EntityMap.ContainsKey(gameObject);
        var entity2GoExist = Entity2GameObjectMap.ContainsKey(entityID);
        if(!go2EntityExist && !entity2GoExist) {
            GameObject2EntityMap.Add(gameObject,entityID);
            Entity2GameObjectMap.Add(entityID,gameObject);
        } else {
            Debug.LogWarning($"Pair Exist:go2EntityExist: {go2EntityExist},entity2GoExist: {entity2GoExist},Use Change instand");
        }
    }

    public void Regist(Entity entity,GameObject gameObject) {
        Regist(entity.Index,gameObject);
    }

    public void UnRegist(GameObject gameObject) {
        if(GameObject2EntityMap.ContainsKey(gameObject)) {
            var entity = GameObject2EntityMap[gameObject];
            if(Entity2GameObjectMap[entity] == gameObject) {
                GameObject2EntityMap.Remove(gameObject);
                Entity2GameObjectMap.Remove(entity);
            } else {
                Debug.LogError($"Entity-GameObject Pair doesn't match,entity of go:{entity},go of entity: {Entity2GameObjectMap[entity]}");
            }
        }
    }

    public void Change(GameObject gameObject,Entity entity) {
        Change(gameObject,entity.Index);
    }

    public void Change(Entity entity,GameObject gameObject) {
        Change(entity.Index,gameObject);
    }

    public void Change(GameObject gameObject,int entityID) {
        if(!gameObject) {
            Debug.LogError("GameObject is null");
            return;
        }
        if(entityID <= 0) {
            Debug.LogError($"Entity is null or inValid,entityID: {entityID}");
            return;
        }
        if(GameObject2EntityMap.ContainsKey(gameObject)) {
            var originID = GameObject2EntityMap[gameObject];
            if(originID == entityID)
                return;
            if(Entity2GameObjectMap[originID] != gameObject) {
                Debug.LogError($"Entity-GameObject Pair doesn't match,go-entity: {originID},entity-go: {Entity2GameObjectMap[originID]}");
                return;
            }
            GameObject2EntityMap[gameObject] = entityID;
            Entity2GameObjectMap.Remove(originID);

            if(Entity2GameObjectMap.ContainsKey(entityID)) {
                var oriGO = Entity2GameObjectMap[entityID];
                Entity2GameObjectMap[entityID] = gameObject;
                GameObject2EntityMap.Remove(oriGO);
                Debug.LogWarning($"Entity:{entityID}->Go map Exist,ori: {oriGO},now has changed to: {gameObject},the GO->Entity map has been dropped");
            } else {
                Entity2GameObjectMap.Add(entityID,gameObject);
            }
        } else {
            Debug.LogError($"go doesn't exist,go: {gameObject}");
        }
    }

    public void Change(int entityID,GameObject gameObject) {
        if(entityID <= 0) {
            Debug.LogError($"Entity is null or inValid, entityID: {entityID}");
            return;
        }
        if(!gameObject) {
            Debug.LogError("GameObject is null");
            return;
        }
        if(Entity2GameObjectMap.ContainsKey(entityID)) {
            var originGO = Entity2GameObjectMap[entityID];
            if(originGO == gameObject)
                return;
            if(GameObject2EntityMap[originGO] != entityID) {
                Debug.LogError($"Entity-GameObject Pair doesn't match,entity-go: {originGO},go-entity: {GameObject2EntityMap[originGO]}");
                return;
            }
            Entity2GameObjectMap[entityID] = gameObject;
            GameObject2EntityMap.Remove(originGO);

            if(GameObject2EntityMap.ContainsKey(gameObject)) {
                var oriEntityID = GameObject2EntityMap[gameObject];
                GameObject2EntityMap[gameObject] = entityID;
                Entity2GameObjectMap.Remove(oriEntityID);
                Debug.LogWarning($"Go:{gameObject}->Entity map Exist,ori: {oriEntityID},now has changed to: {entityID},the Entity->GO map has been dropped");
            } else {
                GameObject2EntityMap.Add(gameObject,entityID);
            }
        } else {
            Debug.LogError($"entity doesn't exist,entityID: {entityID}");
        }
    }

    public void Exchange((GameObject gameObject, int entityID) pair1,(GameObject gameObject, int entityID) pair2) {
        if(!CheckMappingMatch(pair1) || !CheckMappingMatch(pair2)) {
            Debug.LogError($"Mapping not match,pair1: {CheckMappingMatch(pair1)},pair2: {CheckMappingMatch(pair2)}");
            return;
        }
        GameObject2EntityMap[pair1.gameObject] = pair2.entityID;
        GameObject2EntityMap[pair2.gameObject] = pair1.entityID;
        Entity2GameObjectMap[pair1.entityID] = pair2.gameObject;
        Entity2GameObjectMap[pair2.entityID] = pair1.gameObject;
    }

    public bool CheckMappingMatch((GameObject gameObject, int entityID) pair) {
        return GameObject2EntityMap[pair.gameObject] == pair.entityID && Entity2GameObjectMap[pair.entityID] == pair.gameObject;
    }

    public GameObject FindGameObject(int entityID) {
        if(Entity2GameObjectMap.ContainsKey(entityID)) {
            return Entity2GameObjectMap[entityID];
        }
        return null;
    }

    public GameObject FindGameObject(Entity entity) {
        if(Entity2GameObjectMap.ContainsKey(entity.Index)) {
            return Entity2GameObjectMap[entity.Index];
        }
        return null;
    }

    public int FindEntityID(GameObject gameObject) {
        if(gameObject == null) {
            Debug.LogWarning("gameobject is null");
            return 0;
        }
        if(GameObject2EntityMap.ContainsKey(gameObject)) {
            return GameObject2EntityMap[gameObject];
        }
        Debug.LogError($"Doesn't exist this go-entity pair: {gameObject}");
        return 0;
    }
}