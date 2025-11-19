using Drive;
using ECS;
using GAS;
using PoolingSystem.ReferencePool;
using RollBackSystem;
using UnityEngine;
using Utility;
using Component = ECS.Component;

public class CharactorTransformController : Component, ITransformController , IRollBackable {
    #region ITransformController 
    private GameObject gameObject;
    private Transform transform;
    private Rigidbody rigidbody;
    public GameObject GameObject => gameObject;

    // 平滑管理器
    private AttributeSmoothHandler<Vector3> _vector3SmoothHandler;
    private AttributeSmoothHandler<Quaternion> _quaternionSmoothHandler;
    private AttributeSmoothHandler<float> _floatSmoothHandler;

    // 任务ID常量
    private const int POSITION_TASK_ID = 1;
    private const int ROTATION_TASK_ID = 2;
    private const int SCALE_TASK_ID = 3;

    // 逻辑状态(用于回滚和物理)
    private Vector3 _logicPosition;
    private Quaternion _logicRotation = Quaternion.identity;
    private Vector3 _logicScale = Vector3.one;

    // 公开属性
    public Vector3 CurrentPosition => transform.position;
    public Vector3 LogicPosition => _logicPosition;
    public Quaternion CurrentRotation => transform.rotation;
    public Quaternion LogicRotation => _logicRotation;
    public Vector3 CurrentScale => transform.localScale;
    public Vector3 LogicScale => _logicScale;
    public Vector3 Velocity => rigidbody != null ? rigidbody.linearVelocity : Vector3.zero;
    public ControllerTypeEnum Type => ControllerTypeEnum.Transform;

    #region 平滑方法
    public void MoveToSmoothly(Vector3 newPos,int smoothFrameCount) {
        _logicPosition = newPos;
        _vector3SmoothHandler.RegistTask(
                        POSITION_TASK_ID,
                        CurrentPosition,
                        _logicPosition,
                        smoothFrameCount,
                        (visualPos) => transform.position = visualPos,
                        (from,to,t) => Vector3.Lerp(from,to,t),
                        (a,b) => a == b
                    );
    }

    public void RotateToSmoothly(Vector3 newDir,int smoothFrameCount) {
        if(newDir == Vector3.zero)
            return;

        _logicRotation = Quaternion.LookRotation(newDir);

        _quaternionSmoothHandler.RegistTask(
            ROTATION_TASK_ID,
            CurrentRotation,
            _logicRotation,
            smoothFrameCount,
            (visualRot) => transform.rotation = visualRot,
            (from,to,t) => Quaternion.Slerp(from,to,t),
            (a,b)=> a == b
        );
    }

    public void RotateToSmoothly(Quaternion newRot,int smoothFrameCount) {
        _logicRotation = newRot;
        _quaternionSmoothHandler.RegistTask(
            ROTATION_TASK_ID,
            CurrentRotation,
            _logicRotation,
            smoothFrameCount,
            (visualRot) => transform.rotation = visualRot,
            (from,to,t) => Quaternion.Slerp(from,to,t),
            (a,b)=> a == b
        );
    }

    public void LookAtSmoothly(Vector3 point,int smoothFrameCount) {
        Vector3 direction = (point - _logicPosition).normalized;
        if(direction != Vector3.zero) {
            RotateToSmoothly(direction,smoothFrameCount);
        }
    }

    public void ScaleToSmoothly(Vector3 newScale,int smoothFrameCount) {
        _logicScale = newScale;

        _vector3SmoothHandler.RegistTask(
            SCALE_TASK_ID,
            CurrentScale,
            _logicScale,
            smoothFrameCount,
            (visualScale) => transform.localScale = visualScale,
            (from,to,t) => Vector3.Lerp(from,to,t),
            (a,b) => a == b
        );
    }

    #endregion

    #region 直接设置方法
    public void SetPosition(Vector3 newPos) {
        _logicPosition = newPos;
        transform.position = newPos;
        _vector3SmoothHandler.SyncVisualToLogic(POSITION_TASK_ID);
    }

    public void SetRotation(Quaternion newRot) {
        _logicRotation = newRot;
        transform.rotation = newRot;
        _quaternionSmoothHandler.SyncVisualToLogic(ROTATION_TASK_ID);
    }

    public void SetRotation(Vector3 newDir) {
        if(newDir == Vector3.zero)
            return;
        _logicRotation = Quaternion.LookRotation(newDir.normalized);
        transform.rotation = _logicRotation;
        _quaternionSmoothHandler.SyncVisualToLogic(ROTATION_TASK_ID);
    }

    public void SetScale(Vector3 newScale) {
        _logicScale = newScale;
        transform.localScale = newScale;
        _vector3SmoothHandler.SyncVisualToLogic(SCALE_TASK_ID);
    }

    public void LookAt(Vector3 point) {
        Vector3 direction = (point - _logicPosition).normalized;
        if(direction != Vector3.zero) {
            SetRotation(direction);
        }
    }

    public void FaceTo(Vector3 newDir) {
        SetRotation(newDir);
    }

    public void SetLogicPosition(Vector3 newPos) {
        _logicPosition = newPos;
    }
    public void SetLogicRotation(Quaternion newRot) {
        _logicRotation = newRot;
    }
    public void SetLogicScale(Vector3 newScale) {
        _logicScale = newScale;
    }
    public void ClearAllSmoothTasks() {
        _vector3SmoothHandler.Reset();
        _quaternionSmoothHandler.Reset();
        _vector3SmoothHandler.Reset();
    }
    #endregion

    public void Update() {
        float deltaTime = Time.deltaTime;
        _vector3SmoothHandler.Update(deltaTime);
        _quaternionSmoothHandler.Update(deltaTime);
        _floatSmoothHandler.Update(deltaTime);
    }

    public void LateUpdate() {
        
    }

    public void LogicUpdate() {
        
    }
    #endregion

    #region Component
    public override ComponentTypeEnum ComponentType => ComponentTypeEnum.CharactorTransformControllerComponent;

    public override void OnAttach(World world,Entity entity) {
        gameObject = world.GetGameObject(entity);
        transform = gameObject.transform;
        rigidbody = gameObject.GetComponent<Rigidbody>();

        // 初始化逻辑状态
        _logicPosition = transform.position;
        _logicRotation = transform.rotation;
        _logicScale = transform.localScale;

        GizmosDrawer.Instance.RegisterGizmosDrawer(DrawGizmos);
    }

    public override void Reset(World world,Entity entity) {
        _vector3SmoothHandler.Reset();
        _quaternionSmoothHandler.Reset();
        _floatSmoothHandler.Reset();
        gameObject = null;
        transform = null;
        rigidbody = null;
    }

    public override Component GetNewInstance() {
        var res = new CharactorTransformController();
        res._vector3SmoothHandler = new AttributeSmoothHandler<Vector3>();
        res._quaternionSmoothHandler = new AttributeSmoothHandler<Quaternion>();
        res._floatSmoothHandler = new AttributeSmoothHandler<float>();
        return res;
    }

    public override void OnDestroy() {
        _vector3SmoothHandler.Reset();
        _quaternionSmoothHandler.Reset();
        _floatSmoothHandler.Reset();
        gameObject = null;
        transform = null;
        rigidbody = null;
        _vector3SmoothHandler = null;
        _quaternionSmoothHandler = null;
        _floatSmoothHandler = null;
        GizmosDrawer.Instance.UnregisterGizmosSelectedDrawer(DrawGizmos);
    }
    #endregion

    #region Gizmos
    private bool _showGizmos = true;
    private Color _logicPositionColor = Color.green;
    private Color _visualPositionColor = Color.yellow;
    private float _gizmosSphereRadius = 0.2f;

    private void DrawGizmos() {
        if(!_showGizmos || transform == null)
            return;

        // 绘制逻辑位置 (绿色球体)
        Gizmos.color = _logicPositionColor;
        Gizmos.DrawWireSphere(_logicPosition,_gizmosSphereRadius);
        Gizmos.DrawLine(_logicPosition,_logicPosition + Vector3.up * 0.5f);

        // 绘制视觉位置 (黄色球体)
        Gizmos.color = _visualPositionColor;
        Gizmos.DrawWireSphere(transform.position,_gizmosSphereRadius * 0.8f);

        // 绘制逻辑位置到视觉位置的连线
        if(Vector3.Distance(_logicPosition,transform.position) > 0.01f) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_logicPosition,transform.position);
        }

        // 绘制逻辑朝向
        Gizmos.color = Color.blue;
        Vector3 forward = _logicRotation * Vector3.forward;
        Gizmos.DrawRay(_logicPosition,forward * 1.0f);
    }
    #endregion

    #region IRollBackable
    internal class CharactorTransformControllerSnapShot : ISnapShot, IReference<CharactorTransformControllerSnapShot> {
        public Vector3 LogicPosition;
        public Quaternion LogicRotation;
        public Vector3 LogicScale;
        public int LocalizedLogicFrameCount { get; set; }

        uint IReference.ReferenceType => ReferenceTypes.CHARACTORTRANSFORMCONTROLLERSHAPSHOT;

        int IReference.IndexInRefrencePool { get; set; }

        public void Dispose() {
            
        }

        public IReference GetNewInstance() {
            return new CharactorTransformControllerSnapShot();
        }

        public void OnRecycle() {
            
        }

        public void Release() {
            ReferencePoolingCenter.Instance.ReleaseReference(this);
        }
    }

    public ISnapShot SnapShot(int localizedLogicFrameCount) {
        var snapShot = ReferencePoolingCenter.Instance.GetReference<CharactorTransformControllerSnapShot>();
        snapShot.LogicPosition = LogicPosition;
        snapShot.LogicRotation = LogicRotation;
        snapShot.LogicScale = LogicScale;
        snapShot.LocalizedLogicFrameCount = localizedLogicFrameCount;
        return snapShot;
    }

    public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
        var transformSnapShot = snapShot as CharactorTransformControllerSnapShot;
        if(transformSnapShot == null)
            return;
        var count = currentLocalizedLogicFrameCount - transformSnapShot.LocalizedLogicFrameCount;
        int trackBackFrameCount = (int)(count * (1f / Time.deltaTime) / (float)FixedRateScheduler._cfg.RateHz) / 2;
        MoveToSmoothly(transformSnapShot.LogicPosition,trackBackFrameCount);
        RotateToSmoothly(transformSnapShot.LogicRotation,trackBackFrameCount);
        ScaleToSmoothly(transformSnapShot.LogicScale,trackBackFrameCount);
    }
    #endregion
}