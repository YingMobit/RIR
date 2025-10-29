using System;
using System.Net.NetworkInformation;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class PlayerFollowCamController : MonoBehaviour {
    private CinemachineVirtualCamera vcam;
    private CinemachinePOV pov;
    void Start() {
        vcam = GetComponent<CinemachineVirtualCamera>();
        pov = vcam.GetCinemachineComponent<CinemachinePOV>();
    }
    void LateUpdate() {
        TurnCam();
    }

    void TurnCam() {
        float pitch = Mathf.Clamp(CursorAimer.Instance.Pitch.ToFloat(),-50,50);
        pov.m_VerticalAxis.Value = pitch;
    }
}