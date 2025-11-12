using UnityEngine;

namespace GAS {
    /// <summary>
    /// 角色移动接口，要提供多种实现移动的方式，同时需要处理插值
    /// </summary>
    internal interface ITransformController : IController {
        public Vector3 CurrentPosition { get; }
        public Vector3 LogicPosition { get; }
        public Quaternion CurrentRotation { get; }
        public Quaternion LogicRotation { get; }
        public Vector3 CurrentScale { get; }
        public Vector3 LogicScale { get; }

        public Vector3 Velocity { get; }

        public void MoveToSmoothly(Vector3 newPos,int smoothFrameCount);
        public void RotateToSmoothly(Vector3 newDir,int smoothFrameCount);
        public void RotateToSmoothly(Quaternion newRot,int smoothFrameCount);
        public void LookAtSmoothly(Vector3 point,int smoothFrameCount);
        public void ScaleToSmoothly(Vector3 newScale,int smoothFrameCount);
        public void SetPosition(Vector3 newPos);
        public void SetRotation(Quaternion newRot);
        public void SetRotation(Vector3 newDir);
        public void LookAt(Vector3 point);
        public void FaceTo(Vector3 newDir);
        public void SetScale(Vector3 newScale);

        public void SetLogicPosition(Vector3 newPos);
        public void SetLogicRotation(Quaternion newRot);
        public void SetLogicScale(Vector3 newScale);
        public void ClearAllSmoothTasks();
    }
}