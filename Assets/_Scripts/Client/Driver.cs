using ECS;
using Drive;
using TagSystem;
using UnityEngine;
using System.Collections.Generic;
using InputSystemNameSpace;

[DefaultExecutionOrder(int.MinValue)]
public class Driver : MonoBehaviour {
    [SerializeField] ComponentTypeEnum playerComponentType;
    [SerializeField] List<GameObject> CharactorPrefabs;
    
    public World world { get; private set; }
    private int lastNetworkFrameCount = -1;
    private NetworkManager networkManager;
    private int saveNetworkFrameDelay = 3;

    public void StartGame(Dictionary<int,int> playerID_CharactorIDMap) {
        DontDestroyOnLoad(gameObject);
        world = new();
        networkManager = NetworkManager.Instance;
        BuildCharactors(playerID_CharactorIDMap);
        FixedRateScheduler.OnTick += OnUpdate;
        FixedRateScheduler.Start();
    }

    void BuildCharactors(Dictionary<int,int> playerID_CharactorIDMap) {
        foreach(var kvp in playerID_CharactorIDMap) {
            int playerID = kvp.Key;
            int charactorID = kvp.Value;
            GameObject charactorGO = Instantiate(CharactorPrefabs[charactorID],Vector3.up * 4 + Vector3.right * playerID,Quaternion.identity);
            var entity = world.GetEntity(charactorGO,playerComponentType.ToMask());
            world.GetComponentOnEntity(entity , ComponentTypeEnum.InputComponent,out var inputComponent);
            (inputComponent as InputComponent).BindPlayerID(playerID);
        }
    }

    void OnUpdate(long localLogicFrameCount,double deltaTime) {
        Debug.Log($"LocalLogicalFrameCount: {localLogicFrameCount},renderFrameCount: {Time.frameCount}");
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

    private void OnDestroy() {
        FixedRateScheduler.OnTick -= OnUpdate;
    }
}