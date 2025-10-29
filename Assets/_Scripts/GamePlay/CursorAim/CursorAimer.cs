using Lockstep.Math;
using UnityEngine;
using Utility;

public class CursorAimer : Singleton<CursorAimer> {
    [SerializeField] float Sensitivity = 10;

    [field: SerializeField] public LVector3 AimDirection { get; private set; } = LVector3.forward;
    [field: SerializeField] public LFloat Pitch { get; private set; } = LFloat.zero;
    [field: SerializeField] public LFloat Yaw { get; private set; } = LFloat.zero;
    protected override bool _isDonDestroyOnLoad => true;

    protected override void Awake() {
        base.Awake();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    void Update() {
        UpdateAimDirection();
#if UNITY_EDITOR
        DebugAimDir();
#endif
    }

    void UpdateAimDirection() { 
        Yaw += (Input.GetAxis("Mouse X") * Sensitivity).ToLFloat();
        Pitch = Mathf.Clamp(Pitch.ToFloat() - Input.GetAxis("Mouse Y") * Sensitivity,-89f,89f).ToLFloat();
        AimDirection = (Quaternion.Euler(Pitch.ToFloat(),Yaw.ToFloat(),0) * Vector3.forward).normalized.ToLVector3();
    }

    void DebugAimDir() {
        Debug.DrawRay(Vector3.zero,AimDirection.ToVector3(),Color.red);
    }
}
