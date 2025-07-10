using System;
using UnityEngine;
public class CharactorTurner : MonoBehaviour {
    [SerializeField] float turnSpeed = 100f;
    [SerializeField] GameObject Charactor;

    void Awake() {
        Charactor ??= gameObject;
    }

    void Update() {
        TurnPlayer();
    }

    void TurnPlayer() {
        Vector3 aimDir = CursorAimer.Instance.AimDirection;
        aimDir.y = 0; // 保持水平面上的方向
        aimDir.Normalize(); // 确保方向向量是单位向量
        if(aimDir != Vector3.zero) {
            // 计算目标旋转
            Quaternion targetRotation = Quaternion.LookRotation(aimDir);
            // 插值旋转
            Charactor.transform.rotation = Quaternion.Slerp(Charactor.transform.rotation,targetRotation,turnSpeed * Time.deltaTime);
        }
    }
}
