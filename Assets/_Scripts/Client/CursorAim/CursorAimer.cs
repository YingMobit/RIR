using System.Net.WebSockets;
using UnityEngine;
using UnityEngine.AI;
using Utility;

public class CursorAimer : Singleton<CursorAimer> {
    public Vector3 AimDirection { get; private set; } = Vector3.forward;

    [Header("Config")]
    [SerializeField] private float m_Sensitivity = 400;
    [SerializeField] private bool debug = true;
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        UpdateAimDir();
        if(debug)
            Debug.DrawRay(Vector3.zero,AimDirection,Color.red);
    }

    void UpdateAimDir() {
        var move_x = Input.GetAxis("Mouse X") * m_Sensitivity * Time.deltaTime;
        var move_y = Input.GetAxis("Mouse Y") * m_Sensitivity * Time.deltaTime;

        //应用左右
        AimDirection = Quaternion.Euler(0,move_x,0) * AimDirection;
        //应用上下
        move_y = AimDirection.z < 0 ? -move_y : move_y;
        AimDirection = Quaternion.Euler(-move_y,0,0) * AimDirection;
    }
}
