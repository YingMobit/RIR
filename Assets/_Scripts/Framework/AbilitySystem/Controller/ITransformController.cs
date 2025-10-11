using UnityEngine;

namespace GAS {
    /// <summary>
    /// 角色移动接口，要提供多种实现移动的方式，同时需要处理插值
    /// </summary>
    internal interface ITransformController : IController {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Scale { get; }
        public Vector3 Velocity { get; }

        public void MoveTo(Vector3 newPos,float smoothTime);
        public void SetPosition(Vector3 newPos);
        public void RotateTo(Quaternion newRot,float smoothTime);
        public void RotateTo(Vector3 newDir,float smoothTime);
        public void SetRotation(Quaternion newRot);
        public void SetRotation(Vector3 newDir);
        public void LookAtSmoothly(Vector3 point,float smoothTime);
        public void LookAt(Vector3 point);
        public void ScaleTo(Vector3 newScale,float smoothTime);
        public void SetScale(Vector3 newScale);
        public void VelocityTo(Vector3 newSpeed,float smoothTime);
        public void SetVelocity(Vector3 newSpeed);
        public void AddForce(Vector3 force,ForceMode forceMode);
    }
}