using System;
using Sirenix.Serialization;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletConfigData",menuName = "ScriptableObject/BulletConfigData",order = 4)]
public class BulletConfigData : ScriptableObject {
    public float BulletSpeed;
    public float HitRadius;
    public LayerMask HitMask;
    public float MaxLifeTime;
    public GameObject Prefab;
    public BulletConfigData Copy() {
        return new BulletConfigData();
    }
}
