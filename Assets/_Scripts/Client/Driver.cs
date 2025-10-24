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

    void Awake() {
        DontDestroyOnLoad(gameObject);
        world = new();
        networkManager = NetworkManager.Instance;
        BuildCharactors();
        FixedRateScheduler.OnTick += OnUpdate;
        FixedRateScheduler.Start();
    }

    void BuildCharactors() {
        
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