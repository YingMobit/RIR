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
        aimDir.y = 0; // ����ˮƽ���ϵķ���
        aimDir.Normalize(); // ȷ�����������ǵ�λ����
        if(aimDir != Vector3.zero) {
            // ����Ŀ����ת
            Quaternion targetRotation = Quaternion.LookRotation(aimDir);
            // ��ֵ��ת
            Charactor.transform.rotation = Quaternion.Slerp(Charactor.transform.rotation,targetRotation,turnSpeed * Time.deltaTime);
        }
    }
}
