using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Utility;

public class EntityBaker : Singleton<EntityBaker> {
    private EntityManager entityManager;
    private Dictionary<GameObject,Entity> GOPrefabEntityMap = new();
    private bool ready = false;
    public bool Ready => ready;

    protected override void Awake() {
        // Debug.Log("EntityBaker Awake");
        base.Awake();
        StartCoroutine(GetEntityManager());
    }

    IEnumerator GetEntityManager() {
        yield return new WaitWhile(() => World.DefaultGameObjectInjectionWorld == null);
        // Debug.Log("EntityBaker: Entity World Ready");
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        ready = true;
    }

    public Entity GetEntityPrototype(GameObject gameObject) {
        if(GOPrefabEntityMap.ContainsKey(gameObject)) {
            return GOPrefabEntityMap[gameObject];
        } else {
            var prototype = Bake(gameObject);
            GOPrefabEntityMap.Add(gameObject,prototype);
            return prototype;
        }
    }

    private Entity Bake(GameObject prefab) {
        Entity res = entityManager.CreateEntity();
        // entityManager.AddComponent<Prefab>(res);

        var materials = prefab.GetComponent<MeshRenderer>().sharedMaterials;
        Mesh mesh = prefab.GetComponent<MeshFilter>().sharedMesh;

        var filterSettings = RenderFilterSettings.Default;
        filterSettings.ShadowCastingMode = ShadowCastingMode.Off;
        filterSettings.ReceiveShadows = false;
        RenderMeshDescription renderMeshDescription = new() {
            FilterSettings = filterSettings,
            LightProbeUsage = LightProbeUsage.Off,
        };
        RenderMeshArray renderMeshArray = new(materials,new[] { mesh });

        RenderMeshUtility.AddComponents(
            res,
            entityManager,
            renderMeshDescription,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0,0)
        );
        RenderBounds renderBounds = new() { Value = mesh.bounds.ToAABB() };
        entityManager.SetComponentData(res,renderBounds);

        entityManager.AddComponent<LocalTransform>(res);
        LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(res);
        localTransform.Scale = 1;
        entityManager.SetComponentData(res,localTransform);

        entityManager.AddComponent<Prefab>(res);
        return res;
    }
}