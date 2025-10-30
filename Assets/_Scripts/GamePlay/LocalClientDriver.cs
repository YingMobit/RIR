using ECS;
using Drive;
using UnityEngine;
using System.Collections.Generic;
using InputSystemNameSpace;
using Utility;
using GAS;
using ReferencePoolingSystem;

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
        Debug.Log($"BuildCharactors,Charactor Count:{playerID_CharactorIDMap.Count}");
        foreach(var kvp in playerID_CharactorIDMap) {
            int playerID = kvp.Key;
            int charactorID = kvp.Value;
            GameObject charactorGO = CreateGameObject(CharactorPrefabs[charactorID],Vector3.up * 4 + Vector3.right * playerID);
            var entity = world.GetEntity(charactorGO,playerComponentType.ToMask());
            world.GetComponentOnEntity(entity , ComponentTypeEnum.InputComponent,out var inputComponent);
            (inputComponent as InputComponent).BindPlayerID(playerID);
            if(playerID != NetworkManager.Instance.LocalPlayerID) {
                DestroyImmediate(charactorGO.transform.GetChild(1).gameObject);
            }
        }
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
    }

    void OnLateLogicUpdate(int localFrameCount,float deltaTime) {
        world.OnLateUpdate(localFrameCount,deltaTime);
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

    public GameObject CreateGameObject(GameObject prefab,Vector3 position) {
        var res = Instantiate(prefab,position,Quaternion.identity);
        var contextBuilder = res.GetComponent<AbilityComponentContextBuilder>();
        var animationController = world.ReferencePoolingCenter.GetReference<CharactorAnimationController>();
        animationController.BindGameObject(res);
        var transformController = world.ReferencePoolingCenter.GetReference<CharactorTransformController>();
        transformController.BindGameObject(res);
        contextBuilder.RegistController(ControllerTypeEnum.Animation,animationController);
        contextBuilder.RegistController(ControllerTypeEnum.Transform,transformController);
        controllers.Add(animationController);
        controllers.Add(transformController);
        return res;
    }

    public void ReleaseGameObject(GameObject gameobject) { 
        
    }
}