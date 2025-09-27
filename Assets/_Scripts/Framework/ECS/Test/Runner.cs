using System;
using System.Collections.Generic;
using System.Diagnostics;
using ECS;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Runner : MonoBehaviour {
    [Header("Functional Test Settings")]
    public bool autoRunFunctionalOnStart = true;

    [Header("Stress Test Settings (Single Frame Mode)")]
    [Tooltip("ÿ�ִ���ʵ������")] public int stressEntityCount = 2000;
    [Tooltip("ÿ��ʵ��ִ��(���+�Ƴ�)�������")] public int togglesPerEntity = 8;
    [Tooltip("ѹ���������� (ȫ����һ�ΰ��������ĵ�֡�����)")] public int stressRounds = 3;
    [Tooltip("������� (�ɸ���)")] public int randomSeed = 12345;

    [Header("Hotkeys")]
    public KeyCode functionalKey = KeyCode.Q;
    public KeyCode stressKey = KeyCode.W;
    public KeyCode refreshEntityKey = KeyCode.E;

    private World world;
    // �Զ����޷����������xorshift32��
    private uint _randState;
    private void Reseed() { _randState = (uint)(randomSeed == 0 ? 1 : randomSeed) | 1u; }
    private int NextInt(int max) {
        // xorshift32
        _randState ^= _randState << 13;
        _randState ^= _randState >> 17;
        _randState ^= _randState << 5;
        return (int)(_randState % (uint)max);
    }
    // Ԥ���������������飨����ÿ�� new��
    private static readonly ComponentTypeEnum[] STRESS_TYPES = {
        ComponentTypeEnum.Test1,
        ComponentTypeEnum.NewComponent,
        ComponentTypeEnum.HAHAHA
    };

    [Header("Stress Log Options")] public bool logEachFailure = true; // ʧ��ʱ�Ƿ����������־

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

    #region Functional Test
    public void RunFunctionalTest() {
        Debug.Log("===== [Functional Test] ��ʼ =====");
        // 1. ����ʵ��
        var eEmpty = world.GetEntity(gameObject,0);
        var initMask = ComponentTypeEnum.Test1.ToMask() | ComponentTypeEnum.NewComponent.ToMask();
        var eInit = world.GetEntity(gameObject,initMask);
        Debug.Log($"[Step1] eEmpty ID={eEmpty.EntityID} Archetype={Convert.ToString(eEmpty.Archetype,2)} eInit ID={eInit.EntityID} Archetype={Convert.ToString(eInit.Archetype,2)} (ע�⣺Archetype ֻ��ӳ����ʱ״̬)");
        // ������eInit ���ٰ��� Test1 & NewComponent
        Expect(eInit.HasComponent(ComponentTypeEnum.Test1),"Step1 eInit ȱ�� Test1");
        Expect(eInit.HasComponent(ComponentTypeEnum.NewComponent),"Step1 eInit ȱ�� NewComponent");

        // 2. ������ (ֵ���� API����ˢ�¸���)
        world.AddComponent(eEmpty,ComponentTypeEnum.Test1);
        Debug.Log($"[Step2] ���Test1�󣬱��� eEmpty.Archetype(δˢ��)={Convert.ToString(eEmpty.Archetype,2)} (����仯)");
        // ˢ�¸���
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step2-Refresh] ˢ�º� eEmpty.Archetype={Convert.ToString(eEmpty.Archetype,2)} Has(Test1)={eEmpty.HasComponent(ComponentTypeEnum.Test1)}");
        Expect(eEmpty.HasComponent(ComponentTypeEnum.Test1),"Step2 ��� Test1 ��ˢ����δ���� Test1");

        // 3. �ظ����ͬһ���
        var beforePoolCount = world.GetComponents(ComponentTypeEnum.Test1).Count;
        world.AddComponent(eEmpty,ComponentTypeEnum.Test1);
        var afterPoolCount = world.GetComponents(ComponentTypeEnum.Test1).Count;
        Debug.Log($"[Step3] �ظ���� Test1 ������ػ�Ծ����ǰ��: {beforePoolCount}->{afterPoolCount} (Ӧ���)");
        Expect(beforePoolCount == afterPoolCount,"Step3 �ظ���ӵ��³ػ�Ծ�����仯");

        // 4. �����һ�����
        world.AddComponent(eEmpty,ComponentTypeEnum.HAHAHA);
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step4] ����� HAHAHA �� Archetype={Convert.ToString(eEmpty.Archetype,2)} Has(HAHAHA)={eEmpty.HasComponent(ComponentTypeEnum.HAHAHA)}");
        Expect(eEmpty.HasComponent(ComponentTypeEnum.HAHAHA),"Step4 ��� HAHAHA ʧ��");

        // 5. ��ѯ��֤
        var q = world.Query().With(ComponentTypeEnum.Test1);
        Debug.Log($"[Step5] Query With(Test1) Count={q.Entities.Count}");
        Expect(q.Entities.Count > 0,"Step5 Query(Test1) ���Ӧ > 0");
        world.OnLateUpdate(Time.deltaTime); // �ֶ����ղ�ѯ����
        q = world.Query().With(ComponentTypeEnum.Test1).With(ComponentTypeEnum.NewComponent);
        Debug.Log($"[Step5] Query With(Test1,NewComponent) Count={q.Entities.Count}");
        foreach(var ent in q.Entities)
            Expect(ent.HasComponent(ComponentTypeEnum.Test1) && ent.HasComponent(ComponentTypeEnum.NewComponent),"Step5 Query(Test1,NewComponent) ���ڲ���������ʵ��");
        world.OnLateUpdate(Time.deltaTime);
        q = world.Query().With(ComponentTypeEnum.Test1).With(ComponentTypeEnum.NewComponent).Without<HAHAHA>(ComponentTypeEnum.HAHAHA);
        Debug.Log($"[Step5] Query With(Test1,NewComponent) Without(HAHAHA) Count={q.Entities.Count}");
        foreach(var ent in q.Entities)
            Expect(!ent.HasComponent(ComponentTypeEnum.HAHAHA),"Step5 Query Without(HAHAHA) ����԰��� HAHAHA");
        world.OnLateUpdate(Time.deltaTime);

        // 6. Remove ���
        world.RemoveComponent(eEmpty,ComponentTypeEnum.Test1);
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step6] �Ƴ� Test1 �� Has(Test1)={eEmpty.HasComponent(ComponentTypeEnum.Test1)} Archetype={Convert.ToString(eEmpty.Archetype,2)}");
        Expect(!eEmpty.HasComponent(ComponentTypeEnum.Test1),"Step6 �Ƴ� Test1 ʧ��");

        // 7. ������� & �Ƴ�
        uint batchMask = ComponentTypeEnum.Test1.ToMask() | ComponentTypeEnum.NewComponent.ToMask();
        world.AddComponents(eEmpty,batchMask);
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step7] ������Ӻ� Archetype={Convert.ToString(eEmpty.Archetype,2)}");
        Expect(eEmpty.HasComponent(ComponentTypeEnum.Test1) && eEmpty.HasComponent(ComponentTypeEnum.NewComponent),"Step7 �������ȱʧ���");
        world.RemoveComponents(eEmpty,batchMask);
        eEmpty = Refresh(eEmpty);
        Debug.Log($"[Step7] �����Ƴ��� Archetype={Convert.ToString(eEmpty.Archetype,2)}");
        Expect(!eEmpty.HasComponent(ComponentTypeEnum.Test1) && !eEmpty.HasComponent(ComponentTypeEnum.NewComponent),"Step7 �����Ƴ�ʧ��");

        // 8. �ͷ�ʵ�� & Version ����
        short oldVersion = eEmpty.Version;
        world.ReleaseEntity(eEmpty);
        var eAgain = world.GetEntity(gameObject,0);
        Debug.Log($"[Step8] �ͷŲ��������� ʵ��IDһ��={eAgain.EntityID == eEmpty.EntityID} Version��->{oldVersion} ��->{eAgain.Version}");
        Expect(eAgain.EntityID == eEmpty.EntityID,"Step8 ʵ��IDδ���ã���Ӳ�Դ��󣬿�������");
        Expect(eAgain.Version != oldVersion,"Step8 Version δ����");

        Debug.Log("===== [Functional Test] ���� =====");
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
        int totalAddRemove = 0;
        int addFail = 0;
        int removeFail = 0;
        int leftover = 0;
        System.Text.StringBuilder sb = logEachFailure ? null : new System.Text.StringBuilder(256);
        // Ԥ���ݣ��������ڲ����鷭������
        if(tempEntities.Capacity < stressEntityCount)
            tempEntities.Capacity = stressEntityCount;

        for(int round = 0; round < stressRounds; round++) {
            tempEntities.Clear();
            // 1. ����ʵ��(�޳�ʼ���)
            for(int i = 0; i < stressEntityCount; i++) {
                var e = world.GetEntity(gameObject,0); // ����Ҫ gameObject ����ʱ�� null ���� GetInstanceID ����
                tempEntities.Add(new TempEntityInfo { entity = e,mask = 0 });
            }
            // 2. ���/�Ƴ�ѭ��
            for(int i = 0; i < tempEntities.Count; i++) {
                var info = tempEntities[i];
                for(int t = 0; t < togglesPerEntity; t++) {
                    var type = STRESS_TYPES[NextInt(STRESS_TYPES.Length)];
                    world.AddComponent(info.entity,type);
                    var latest = world.GetLatestEntity(info.entity.EntityID);
                    if(!latest.HasComponent(type)) {
                        addFail++;
                        if(logEachFailure)
                            Debug.LogError($"[Stress][Round {round}] AddFail entity={latest.EntityID} type={type}");
                        else
                            sb.Append("AddFail e=").Append(latest.EntityID).Append(' ').Append(type).Append('\n');
                    }
                    world.RemoveComponent(latest,type);
                    var after = world.GetLatestEntity(info.entity.EntityID);
                    if(after.HasComponent(type)) {
                        removeFail++;
                        if(logEachFailure)
                            Debug.LogError($"[Stress][Round {round}] RemoveFail entity={after.EntityID} type={type}");
                        else
                            sb.Append("RemoveFail e=").Append(after.EntityID).Append(' ').Append(type).Append('\n');
                    }
                    totalAddRemove += 2;
                }
            }
            // 3. ����ǰУ��
            foreach(var info in tempEntities) {
                var latest = world.GetLatestEntity(info.entity.EntityID);
                if(latest.Archetype != 0) {
                    leftover++;
                    if(logEachFailure)
                        Debug.LogError($"[Stress][Round {round}] Leftover entity={latest.EntityID} arch={latest.Archetype}");
                    else
                        sb.Append("Leftover e=").Append(latest.EntityID).Append(' ').Append(latest.Archetype).Append('\n');
                }
                world.ReleaseEntity(latest);
            }
        }
        stopwatch.Stop();
        if(!logEachFailure && sb != null && sb.Length > 0)
            Debug.LogError(sb.ToString());
        Debug.Log($"[Stress] ��֡��� {stressRounds} �� ʵ��/��={stressEntityCount} togglesPerEntity={togglesPerEntity} Ops={totalAddRemove} AddFail={addFail} RemoveFail={removeFail} Leftover={leftover} ��ʱ={stopwatch.ElapsedMilliseconds} ms");
    }
    #endregion
}
