using GAS;
using ReferencePoolingSystem;
using UnityEngine;

public class CharactorTransformController : ITransformController , IReference<CharactorTransformController> {
    public Vector3 Position => transform.position;

    public Quaternion Rotation => transform.rotation;

    public Vector3 Scale => transform.localScale;

    public Vector3 Velocity => rigidbody.linearVelocity;

    public ControllerTypeEnum Type => ControllerTypeEnum.Transform;

    public void AddForce(Vector3 force,ForceMode forceMode) {
        rigidbody.AddForce(force,forceMode);
    }

    public void LookAt(Vector3 point) {
        transform.LookAt(point);
    }

    public void LookAtSmoothly(Vector3 point,float smoothTime) {
        vector3SmoothHandler.RegistTask(ROTATIONSMOOTH_TASK_ID,transform.forward,(point - transform.position).normalized,smoothTime,(v)=> {
            transform.rotation = Quaternion.LookRotation(v);
        },(init,target,t) => { 
            return Vector3.Slerp(init,target,t);
        });
    }

    public void MoveTo(Vector3 newPos,float smoothTime) {
        vector3SmoothHandler.RegistTask(POSITIONSMOOTH_TASK_ID,transform.position,newPos,smoothTime,(v)=> {
            rigidbody.MovePosition(v);
        },(init,target,t) => { 
            return Vector3.Lerp(init,target,t);
        });
    }
    public void SetPosition(Vector3 newPos) {
        rigidbody.MovePosition(newPos);
    }

    public void RotateTo(Vector3 newDir,float smoothTime) {
        vector3SmoothHandler.RegistTask(ROTATIONSMOOTH_TASK_ID,transform.forward,newDir.normalized,smoothTime,(v)=> {
            transform.forward = v;
        },(init,target,t) => { 
            return Vector3.Slerp(init,target,t);
        });
    }

    public void FaceTo(Vector3 newDir) {
        transform.forward = newDir.normalized;
    }

    public void ScaleTo(Vector3 newScale,float smoothTime) {
        vector3SmoothHandler.RegistTask(SCALESMOOTH_TASK_ID,transform.localScale,newScale,smoothTime,(v)=> {
            transform.localScale = v;
        },(init,target,t) => { 
            return Vector3.Lerp(init,target,t);
        });
    }

    public void SetRotation(Quaternion newRot) {
        rigidbody.MoveRotation(newRot);
    }

    public void SetRotation(Vector3 newDir) {
        rigidbody.MoveRotation(Quaternion.LookRotation(newDir));
    }

    public void SetScale(Vector3 newScale) {
        transform.localScale = newScale;
    }
    public void VelocityTo(Vector3 newSpeed,float smoothTime) {
        vector3SmoothHandler.RegistTask(VELOCITYSMOOTH_TASK_ID,rigidbody.linearVelocity,newSpeed,smoothTime,(v)=> {
            rigidbody.linearVelocity = v;
        },(init,target,t) => { 
            return Vector3.Lerp(init,target,t);
        });
    }

    public void SetVelocity(Vector3 newSpeed) {
        rigidbody.linearVelocity = newSpeed;
    }

    public void HorizontalVelocityTo(Vector2 newHorizontalSpeed,float smoothTime) {
        vector2SmoothHandler.RegistTask(VELOCITYSMOOTH_TASK_ID,new Vector2(rigidbody.linearVelocity.x,rigidbody.linearVelocity.z),newHorizontalSpeed,smoothTime,(v)=> {
            rigidbody.linearVelocity = new Vector3(v.x,rigidbody.linearVelocity.y,v.y);
        },(init,target,t) => { 
            return Vector2.Lerp(init,target,t);
        });
    }

    public void SetHorizontalVelocity(Vector2 newHorizontalSpeed) {
        rigidbody.linearVelocity = new Vector3(newHorizontalSpeed.x,rigidbody.linearVelocity.y,newHorizontalSpeed.y);
    }

    public GameObject GameObject { get => gameObject; }
    public uint ReferenceType => ReferenceTypes.CHARACTORTRANSFORMCONTROLLER;
    int IReference.IndexInRefrencePool { get; set; }


    private GameObject gameObject;
    private Transform transform;
    private Rigidbody rigidbody;
    private ValueSmoothHandler<Vector2> vector2SmoothHandler;
    private ValueSmoothHandler<Vector3> vector3SmoothHandler;
    private ValueSmoothHandler<Quaternion> quaternionSmoothHandler;

    private const int ROTATIONSMOOTH_TASK_ID = 1;
    private const int POSITIONSMOOTH_TASK_ID = 2;
    private const int SCALESMOOTH_TASK_ID = 3;
    private const int VELOCITYSMOOTH_TASK_ID = 4;

    public void BindGameObject(GameObject gameObject) {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public void Update() {
        vector2SmoothHandler.Update();
        vector3SmoothHandler.Update();
        quaternionSmoothHandler.Update();
    }

    public void LateUpdate() {
        
    }

    public void OnRecycle() {
        vector2SmoothHandler.Reset();
        vector3SmoothHandler.Reset();
        quaternionSmoothHandler.Reset();
        gameObject = null;
        transform = null;
        rigidbody = null;
    }

    public IReference Clone() {
        var res = new CharactorTransformController();
        res.vector2SmoothHandler = new ValueSmoothHandler<Vector2>();
        res.vector3SmoothHandler = new ValueSmoothHandler<Vector3>();
        res.quaternionSmoothHandler = new ValueSmoothHandler<Quaternion>();
        return res as IReference;
    }

    public void Dispose() {
        OnRecycle();
        vector2SmoothHandler = null;
        vector3SmoothHandler = null;
        quaternionSmoothHandler = null;
    }
}
