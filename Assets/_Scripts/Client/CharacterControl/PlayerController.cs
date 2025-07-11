using System;
using UnityEngine;
//�����࣬���ڹ���һ�����ʵ���ϵ��������
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
