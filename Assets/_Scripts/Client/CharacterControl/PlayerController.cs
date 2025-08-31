using System;
using Unity.Entities;
using UnityEngine;
//�����࣬���ڹ���һ�����ʵ���ϵ��������
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
