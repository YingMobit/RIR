using System;
using Unity.Entities;
using UnityEngine;
//容器类，用于管理一个玩家实体上的所有组件
public class PlayerController : MonoBehaviour {
    [SerializeField] private PlayerAbilityConfigData PlayerAbilityConfigData;
    public CharactorActionFSM charactorActionFSM;
    public CharactorMovementFSM charactorMovementFSM;
    public PlayerRuntimeAbilityData playerRuntimeAbilityData;
    public IShootable Weapon;

    private Entity entity;
    public Entity Entity => entity;
    void Awake() {
        charactorActionFSM = GetComponent<CharactorActionFSM>();
        charactorMovementFSM = GetComponent<CharactorMovementFSM>();
        playerRuntimeAbilityData = new(PlayerAbilityConfigData) {
            playerController = this
        };

        entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
        GameObjectEntityMappingSystem.Instance.Regist(entity,gameObject);

        Weapon = GetComponentInChildren<IShootable>();
    }
}
