using System;
using UnityEngine;
//容器类，用于管理一个玩家实体上的所有组件
public class PlayerController : MonoBehaviour {
    public CharactorActionFSM charactorActionFSM;
    public CharactorMovementFSM charactorMovementFSM;
    public PlayerRuntimeAbilityData playerRuntimeAbilityData;
    [SerializeField] private PlayerAbilityConfigData PlayerAbilityConfigData;

    void Awake() {
        charactorActionFSM = GetComponent<CharactorActionFSM>();
        charactorMovementFSM = GetComponent<CharactorMovementFSM>();
        playerRuntimeAbilityData = new(PlayerAbilityConfigData) {
            playerController = this
        };
    }
}
