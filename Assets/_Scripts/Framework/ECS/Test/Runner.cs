using System;
using System.Collections.Generic;
using System.Diagnostics;
using ECS;
using UnityEngine;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

public class Runner : MonoBehaviour {
    [Header("Functional Test Settings")]
    public bool autoRunFunctionalOnStart = true;

    [Header("Stress Test Settings (Single Frame Mode)")]
    [Tooltip("每轮创建实体数量")] public int stressEntityCount = 2000;
    [Tooltip("每个实体执行(添加+移除)组件次数")] public int togglesPerEntity = 8;
    [Tooltip("压力测试轮数 (全部在一次按键触发的单帧内完成)")] public int stressRounds = 3;
    [Tooltip("随机种子 (可复现)")] public int randomSeed = 12345;

    [Header("Hotkeys")]
    public KeyCode functionalKey = KeyCode.Q;
    public KeyCode stressKey = KeyCode.W;
    public KeyCode refreshEntityKey = KeyCode.E;

    private World world;
    private Query query;
    // 自定义无分配随机数（xorshift32）
    private uint _randState;
    private void Reseed() { _randState = (uint)(randomSeed == 0 ? 1 : randomSeed) | 1u; }
    private int NextInt(int max) {
        // xorshift32
        _randState ^= _randState << 13;
        _randState ^= _randState >> 17;
        _randState ^= _randState << 5;
        return (int)(_randState % (uint)max);
    }
    // 预分配的组件类型数组（避免每次 new）
    private static readonly ComponentTypeEnum[] STRESS_TYPES = {
        ComponentTypeEnum.Test1,
        ComponentTypeEnum.NewComponent,
        ComponentTypeEnum.HAHAHA
    };

    [Header("Stress Log Options")] public bool logEachFailure = true; // 失败时是否立即输出日志

    private struct TempEntityInfo { public Entity entity; public uint mask; }
    private List<TempEntityInfo> tempEntities = new();
    private Stopwatch stopwatch = new();

    void Awake() {
        world = new World();
        Reseed();
    }

    void Start() {
        if(autoRunFunctionalOnStart)
            RunFunctionalTest();
    }

    void Update() {
        if(Input.GetKey(functionalKey))
            RunFunctionalTest();
        if(Input.GetKey(stressKey))
            RunStressTestSingleFrame();
    }

    private void LateUpdate() {
        world.OnLateUpdate(Time.deltaTime);
    }

    #region Functional Test
    public void RunFunctionalTest() {
        Debug.Log("===== [Functional Test] 开始 =====");
        // 1. 创建实体
        var eEmpty = world.GetEntity(gameObject,0);
        var initMask = ComponentTypeEnum.Test1.ToMask() | ComponentTypeEnum.NewComponent.ToMask();
        var eInit = world.GetEntity(gameObject,initMask);
        Debug.Log($"[Step1] eEmpty ID={eEmpty.EntityID} Archetype={Convert.ToString(eEmpty.Archetype,2)} eInit ID={eInit.EntityID} Archetype={Convert.ToString(eInit.Archetype,2)} (注意：Archetype 只反映创建时状态)");
        // 期望：eInit 至少包含 Test1 & NewComponent
        Expect(eInit.HasComponent(ComponentTypeEnum.Test1),"Step1 eInit 缺少 Test1");
        Expect(eInit.HasComponent(ComponentTypeEnum.NewComponent),"Step1 eInit 缺少 NewComponent");

        // 2. 添加组件 (值传递 API，不刷新副本)
        world.AddComponent(eEmpty,ComponentTypeEnum.Test1);
        Debug.Log($"[Step2] 添加Test1后，本地 eEmpty.Archetype(未刷新)={Convert.ToString(eEmpty.Archetype,2)} (不会变化)");
        // 刷新副本
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step2-Refresh] 刷新后 eEmpty.Archetype={Convert.ToString(eEmpty.Archetype,2)} Has(Test1)={eEmpty.HasComponent(ComponentTypeEnum.Test1)}");
        Expect(eEmpty.HasComponent(ComponentTypeEnum.Test1),"Step2 添加 Test1 后刷新仍未包含 Test1");

        // 3. 重复添加同一组件
        var list = ListPool<ECS.Component>.Get();
        world.GetComponents(ComponentTypeEnum.Test1,out list);
        var beforePoolCount = list.Count;
        list.Clear();
        world.AddComponent(eEmpty,ComponentTypeEnum.Test1);
        world.GetComponents(ComponentTypeEnum.Test1,out list);
        var afterPoolCount = list.Count;
        ListPool<ECS.Component>.Release(list);
        Debug.Log($"[Step3] 重复添加 Test1 组件，池活跃数量前后: {beforePoolCount}->{afterPoolCount} (应相等)");
        Expect(beforePoolCount == afterPoolCount,"Step3 重复添加导致池活跃数量变化");

        // 4. 添加另一个组件
        world.AddComponent(eEmpty,ComponentTypeEnum.HAHAHA);
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step4] 再添加 HAHAHA 后 Archetype={Convert.ToString(eEmpty.Archetype,2)} Has(HAHAHA)={eEmpty.HasComponent(ComponentTypeEnum.HAHAHA)}");
        Expect(eEmpty.HasComponent(ComponentTypeEnum.HAHAHA),"Step4 添加 HAHAHA 失败");

        // 5. 查询验证
        var q = world.Query().With(ComponentTypeEnum.Test1);
        Debug.Log($"[Step5] Query With(Test1) Count={q.Entities.Count}");
        Expect(q.Entities.Count > 0,"Step5 Query(Test1) 结果应 > 0");
        world.OnLateUpdate(Time.deltaTime); // 手动回收查询对象
        q = world.Query().With(ComponentTypeEnum.Test1).With(ComponentTypeEnum.NewComponent);
        Debug.Log($"[Step5] Query With(Test1,NewComponent) Count={q.Entities.Count}");
        foreach(var ent in q.Entities)
            Expect(ent.HasComponent(ComponentTypeEnum.Test1) && ent.HasComponent(ComponentTypeEnum.NewComponent),"Step5 Query(Test1,NewComponent) 存在不符合条件实体");
        world.OnLateUpdate(Time.deltaTime);
        q = world.Query().With(ComponentTypeEnum.Test1).With(ComponentTypeEnum.NewComponent).Without<HAHAHA>(ComponentTypeEnum.HAHAHA);
        Debug.Log($"[Step5] Query With(Test1,NewComponent) Without(HAHAHA) Count={q.Entities.Count}");
        foreach(var ent in q.Entities)
            Expect(!ent.HasComponent(ComponentTypeEnum.HAHAHA),"Step5 Query Without(HAHAHA) 结果仍包含 HAHAHA");
        world.OnLateUpdate(Time.deltaTime);

        // 6. Remove 组件
        world.RemoveComponent(eEmpty,ComponentTypeEnum.Test1);
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step6] 移除 Test1 后 Has(Test1)={eEmpty.HasComponent(ComponentTypeEnum.Test1)} Archetype={Convert.ToString(eEmpty.Archetype,2)}");
        Expect(!eEmpty.HasComponent(ComponentTypeEnum.Test1),"Step6 移除 Test1 失败");

        // 7. 批量添加 & 移除
        uint batchMask = ComponentTypeEnum.Test1.ToMask() | ComponentTypeEnum.NewComponent.ToMask();
        world.AddComponents(eEmpty,batchMask);
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step7] 批量添加后 Archetype={Convert.ToString(eEmpty.Archetype,2)}");
        Expect(eEmpty.HasComponent(ComponentTypeEnum.Test1) && eEmpty.HasComponent(ComponentTypeEnum.NewComponent),"Step7 批量添加缺失组件");
        world.RemoveComponents(eEmpty,batchMask);
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step7] 批量移除后 Archetype={Convert.ToString(eEmpty.Archetype,2)}");
        Expect(!eEmpty.HasComponent(ComponentTypeEnum.Test1) && !eEmpty.HasComponent(ComponentTypeEnum.NewComponent),"Step7 批量移除失败");

        // 8. 释放实体 & Version 递增
        short oldVersion = eEmpty.Version;
        world.ReleaseEntity(eEmpty);
        var eAgain = world.GetEntity(gameObject,0);
        Debug.Log($"[Step8] 释放并重新申请 实体ID一致={eAgain.EntityID == eEmpty.EntityID} Version旧->{oldVersion} 新->{eAgain.Version}");
        Expect(eAgain.EntityID == eEmpty.EntityID,"Step8 实体ID未复用（非硬性错误，可视需求）");
        Expect(eAgain.Version != oldVersion,"Step8 Version 未递增");

        Debug.Log("===== [Functional Test] 结束 =====");
    }
    #endregion

    private Entity Refresh(Entity e) => world.GetLatestEntity(e.EntityID);

    private void Expect(bool condition,string message) {
        if(!condition)
            Debug.LogError(message);
    }

    #region Stress Test
    public void RunStressTestSingleFrame() {
        Reseed();
        stopwatch.Restart();
        int totalQueries = 0;
        int leftover = 0;
        System.Text.StringBuilder sb = logEachFailure ? null : new System.Text.StringBuilder(256);

        // 预扩容，避免多次内部数组翻倍分配
        if(tempEntities.Capacity < stressEntityCount)
            tempEntities.Capacity = stressEntityCount;

        tempEntities.Clear();

        // 1. 创建实体并为每个实体添加 0~3 个不重复组件
        for (int i = 0; i < stressEntityCount; i++) {
            var e = world.GetEntity(gameObject, 0);
            int toAdd = NextInt(4); // 0..3
            uint combinedMask = 0;
            // 从 STRESS_TYPES 随机挑选 toAdd 个不重复项
            if (toAdd > 0) {
                // 简单用索引数组并随机洗牌的方式选择子集
                int n = STRESS_TYPES.Length;
                int[] idx = new int[n];
                for (int k = 0; k < n; k++) idx[k] = k;
                // 部分洗牌（Fisher–Yates 前 toAdd 项）
                for (int k = 0; k < toAdd; k++) {
                    int r = k + NextInt(n - k);
                    int tmp = idx[k]; idx[k] = idx[r]; idx[r] = tmp;
                }
                for (int k = 0; k < toAdd; k++) {
                    var type = STRESS_TYPES[idx[k]];
                    // 添加组件（AddComponent 已处理重复添加的安全性）
                    world.AddComponent(e, type);
                    combinedMask |= type.ToMask();
                }
            }
            tempEntities.Add(new TempEntityInfo { entity = world.GetLatestEntity(e.EntityID), mask = combinedMask });
        }

        // 2. 查询阶段：对单/双/三组件组合分别查询若干次
        int queryRepeats = Math.Max(1, togglesPerEntity);

        // 单组件查询
        for(int r = 0; r < queryRepeats; r++) {
            foreach(var t in STRESS_TYPES) {
                var q = world.Query().With(t);
                // optionally validate a bit (lightweight)
                if(q.Entities.Count < 0) { }
                totalQueries++;
                world.OnLateUpdate(Time.deltaTime);
            }
        }

        // 双组件组合查询（所有二元组合）
        for(int r = 0; r < queryRepeats; r++) {
            for(int i = 0; i < STRESS_TYPES.Length; i++) {
                for(int j = i+1; j < STRESS_TYPES.Length; j++) {
                    var a = STRESS_TYPES[i];
                    var b = STRESS_TYPES[j];
                    var q = world.Query().With(a).With(b);
                    totalQueries++;
                    world.OnLateUpdate(Time.deltaTime);
                }
            }
        }

        // 三组件组合查询（若有至少3种）
        if(STRESS_TYPES.Length >= 3) {
            for(int r = 0; r < queryRepeats; r++) {
                // take first three as the triple combination (or all triples if more types exist)
                for(int i = 0; i < STRESS_TYPES.Length; i++) {
                    for(int j = i+1; j < STRESS_TYPES.Length; j++) {
                        for(int k = j+1; k < STRESS_TYPES.Length; k++) {
                            var q = world.Query().With(STRESS_TYPES[i]).With(STRESS_TYPES[j]).With(STRESS_TYPES[k]);
                            totalQueries++;
                            world.OnLateUpdate(Time.deltaTime);
                        }
                    }
                }
            }
        }

        // 3. 移除所有组件并回收实体
        foreach(var info in tempEntities) {
            var latest = world.GetLatestEntity(info.entity.EntityID);
            if(latest.Archetype != 0) {
                // 移除所有组件
                world.RemoveComponents(latest, latest.Archetype);
            }
            world.ReleaseEntity(world.GetLatestEntity(info.entity.EntityID));
        }

        stopwatch.Stop();
        Debug.Log($"[Stress] 创建 {stressEntityCount} 实体，执行 queries={totalQueries} 耗时={stopwatch.ElapsedMilliseconds} ms");
    }
    #endregion
}
