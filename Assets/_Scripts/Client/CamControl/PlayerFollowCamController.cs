using System;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class PlayerFollowCamController : MonoBehaviour {
    private CinemachinePOV pov;
    private CinemachineVirtualCamera vcam;
    private Transform playerTransform;

    void Start() {
        vcam = GetComponent<CinemachineVirtualCamera>();
        pov = vcam.GetCinemachineComponent<CinemachinePOV>();

        if(vcam != null && vcam.Follow != null) {
            playerTransform = vcam.Follow;
        }

        // 禁用 POV 的输入轴，改为代码控制
        if(pov != null) {
            pov.m_HorizontalAxis.m_InputAxisName = "";
            pov.m_VerticalAxis.m_InputAxisName = "";
        }
    }
    void LateUpdate() {
        TurnCam();
    }

    void TurnCam() {
        var dir = CursorAimer.Instance.AimDirection;
        var angle_vectical = -Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        var angle_horizontal = Mathf.Atan2(dir.x,dir.z) * Mathf.Rad2Deg;
        pov.m_VerticalAxis.Value = Mathf.LerpAngle(pov.m_VerticalAxis.Value,angle_vectical,pov.m_VerticalAxis.m_MaxSpeed * Time.deltaTime);
        pov.m_HorizontalAxis.Value = Mathf.LerpAngle(pov.m_HorizontalAxis.Value,angle_horizontal,pov.m_HorizontalAxis.m_MaxSpeed * Time.deltaTime);
    }
}