using ECS;
using Scheduling;
using UnityEngine;
using Utility;

[DefaultExecutionOrder(int.MinValue)]
public class Driver : Singleton<Driver> {
    public World world { get; private set; }
    public GameObject Player { get; private set; }
    protected override bool _isDonDestroyOnLoad => true;

    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] ComponentTypeEnum playerComponentType;

    protected override void Awake() {
        base.Awake();
        world = new();
        Player = Instantiate(PlayerPrefab,Vector3.up * 4,Quaternion.identity);
        BuildEntity(Player,playerComponentType.ToMask());
        FixedRateScheduler.OnTick += OnUpdate;
        FixedRateScheduler.Start();
    }

    public void BuildEntity(GameObject gameObject,uint componentType) {
        world.GetEntity(gameObject,componentType);
    }

    void OnUpdate(long localFrameCount,double deltaTime) {
        OnLogicUpdate((int)localFrameCount,(float)deltaTime);
        OnLateLogicUpdate((int)localFrameCount,(float)deltaTime);
        OnNetworUpdate((int)localFrameCount);
    }

    void OnLogicUpdate(int localFrameCount,float deltaTime) {
        world.OnUpdate(localFrameCount,deltaTime);
    }

    void OnLateLogicUpdate(int localFrameCount,float deltaTime) { 
        world.OnLateUpdate(localFrameCount,deltaTime);
    }

    void OnNetworUpdate(int networkFrameCount) { 
        world.OnNetworkUpdate(networkFrameCount);
    }

    private void OnDestroy() {
        FixedRateScheduler.OnTick -= OnUpdate;
    }
}