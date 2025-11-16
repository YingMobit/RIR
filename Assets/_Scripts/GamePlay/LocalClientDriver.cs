using System.Collections.Generic;
using Drive;
using ECS;
using GAS;
using InputSystemNameSpace;
using PoolingSystem.GameObjectPool;
using PoolingSystem.ReferencePool;
using RollBackSystem;
using UnityEngine;
using UnityEngine.Pool;
using Utility;

[DefaultExecutionOrder(int.MinValue)]
public class LocalClientDriver : Singleton<LocalClientDriver> {
    [SerializeField] ComponentTypeEnum playerComponentType;
    [SerializeField] List<GameObject> CharactorPrefabs;
    HashSet<IController> controllers = new();

    public World world { get; private set; }

    public void StartGame(Dictionary<int,int> playerID_CharactorIDMap) {
        DontDestroyOnLoad(gameObject);
        world = new();
        BuildCharactors(playerID_CharactorIDMap);
        FixedRateScheduler.OnTick += OnUpdate;
        FixedRateScheduler.Start();
    }

    void BuildCharactors(Dictionary<int,int> playerID_CharactorIDMap) {
        var compList = ListPool<ECS.Component>.Get();
        foreach(var kvp in playerID_CharactorIDMap) {
            int playerID = kvp.Key;
            int charactorID = kvp.Value;
            GameObject charactorGO = GameObjectPoolCenter.Instance.GetInstance(CharactorPrefabs[charactorID],Vector3.up * 4 + Vector3.right * playerID,Quaternion.identity);
            var entity = world.GetEntity(charactorGO,playerComponentType.ToMask());
            world.GetAllComponentsOnEntity(entity, compList);
            foreach(var comp in compList) { 
                if(comp is InputComponent input) { 
                    input.BindPlayerID(playerID);
                } else if(comp is IController controller) { 
                    controllers.Add(controller);
                }
            }
            if(playerID != NetworkManager.Instance.LocalPlayerID) {
                DestroyImmediate(charactorGO.transform.GetChild(1).gameObject);
            }
            compList.Clear();
        }
        ListPool<ECS.Component>.Release(compList);
    }

    void OnUpdate(long localLogicFrameCount,double deltaTime) {
        OnNetworkUpdate(localLogicFrameCount,deltaTime);
        OnLogicUpdate((int)localLogicFrameCount,(float)deltaTime);
        OnLateLogicUpdate((int)localLogicFrameCount,(float)deltaTime);
    }

    void OnNetworkUpdate(long localLogicFrameCount,double deltaTime) {

    }

    void OnLogicUpdate(int localFrameCount,float deltaTime) {
        world.OnUpdate(localFrameCount,deltaTime);
        foreach(var controller in controllers) {
            controller.LogicUpdate();
        }
        Physics.Simulate(deltaTime);
    }

    void OnLateLogicUpdate(int localFrameCount,float deltaTime) {
        world.OnLateUpdate(localFrameCount,deltaTime);

        //���Ԥ��??
        if(!world.GetSystemByType<InputSystem>().IsPredictCorrect(world,out var errorStartFrameCount)) {
            //�ع�
            world.GetSystemByType<RollBackSystem.RollBackSystem>().RollBack(world,errorStartFrameCount,localFrameCount);
            //����ģ��
            for(int i = 0; i < localFrameCount - errorStartFrameCount + 1; i++) {
                world.OnRollingBack(errorStartFrameCount,errorStartFrameCount + i,deltaTime);
            }
        }
    }

    private void Update() {
        foreach(var controller in controllers) {
            controller.Update();
        }
    }

    private void LateUpdate() {
        foreach(var controller in controllers) {
            controller.LateUpdate();
        }
    }

    private void OnDestroy() {
        FixedRateScheduler.OnTick -= OnUpdate;
    }

    public void ReleaseGameObject(GameObject gameobject) {

    }
}