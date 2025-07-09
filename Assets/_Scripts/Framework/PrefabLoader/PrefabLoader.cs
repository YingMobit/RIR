using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;
public class PrefabLoader : Singleton<PrefabLoader>
{
    private static string _rootPath = "Prefabs/";
    protected override bool _isDonDestroyOnLoad => true;
    private Dictionary<Type, GameObject> Prefabs = new();


    public IPrefab<PrefabType> LoadPrefab<PrefabType>() where PrefabType : IPrefab<PrefabType> {
        Type _type = typeof(PrefabType);
        if(Prefabs.ContainsKey(_type)){
            IPrefab<PrefabType> template = Prefabs[_type].GetComponent<IPrefab<PrefabType>>();
            if(template == null) {
                Debug.LogError($"Prefab Type not match,expected type: {_type.Name}, actual type: {template.Type.Name}");
                return null;
            }
            return template;
        } else {
            Debug.LogError($"Prefab of tyep: {_type} has not been registed");
            return null;
        }
    }

    protected override void Awake() {
        base.Awake();
        Initialize();
    }

    private void Initialize() {
        List<GameObject> gos = Resources.LoadAll<GameObject>(_rootPath).ToList<GameObject>();
        IPrefab prefab;
        foreach (GameObject go in gos) { 
            prefab = go.GetComponent<IPrefab>();
            if(prefab != null){
                Prefabs.Add(prefab.Type,go);
            } else {
                Debug.LogWarning($"Thers's no class attached to the prefab: {go.name} implemented inteface 'IPrefab'");
            }
        }
    }
}
