using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Rendering;
using UnityEngine;
using Utility;

public class MeshMaterialRegist : Singleton<MeshMaterialRegist> {
    private Dictionary<Material,int> materials = new();
    private Dictionary<Mesh,int> meshs = new();
    protected EntitiesGraphicsSystem entityGraphicsSystem;

    protected override void Awake() {
        base.Awake();
        entityGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();
    }

    public int RegistMaterial(Material material) {
        if(materials.ContainsKey(material)) {
            return materials[material];
        }
        var res = (int)entityGraphicsSystem.RegisterMaterial(material).value;
        materials.Add(material,res);
        return res;
    }

    public int RegistMesh(Mesh mesh) {
        if(meshs.ContainsKey(mesh)) {
            return meshs[mesh];
        }
        var res = (int)entityGraphicsSystem.RegisterMesh(mesh).value;
        meshs.Add(mesh,res);
        return res;
    }
}