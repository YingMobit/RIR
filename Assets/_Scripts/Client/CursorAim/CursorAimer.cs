using UnityEngine;
using Utility;

public class CursorAimer : Singleton<CursorAimer> {
    [SerializeField] float Sensitivity = 10;

    [field: SerializeField] public Vector3 AimDirection { get; private set; } = Vector3.forward;
    [field: SerializeField] public float Pitch { get; private set; } = 0;
    [field: SerializeField] public float Yaw { get; private set; } = 0;
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
        Yaw += Input.GetAxis("Mouse X") * Sensitivity;
        Pitch = Mathf.Clamp(Pitch - Input.GetAxis("Mouse Y") * Sensitivity,-89f,89f);
        AimDirection = (Quaternion.Euler(Pitch,Yaw,0) * Vector3.forward).normalized;
    }

    void DebugAimDir() {
        Debug.DrawRay(Vector3.zero,AimDirection,Color.red);
    }
}
