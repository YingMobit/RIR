using GAS;
using UnityEngine;

public class CharactorTransformController : MonoBehaviour, ITransformController {
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

    public void RotateTo(Quaternion newRot,float smoothTime) {
        quaternionSmoothHandler.RegistTask(ROTATIONSMOOTH_TASK_ID,transform.rotation,newRot,smoothTime,(v)=> {
            rigidbody.MoveRotation(v);
        },(init,target,t) => { 
            return Quaternion.Slerp(init,target,t);
        });
    }

    public void RotateTo(Vector3 newDir,float smoothTime) {
        vector3SmoothHandler.RegistTask(ROTATIONSMOOTH_TASK_ID,transform.forward,newDir.normalized,smoothTime,(v)=> {
            rigidbody.MoveRotation(Quaternion.LookRotation(v));
        },(init,target,t) => { 
            return Vector3.Slerp(init,target,t);
        });
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


    [SerializeField] private new Transform transform;
    [SerializeField] private new Rigidbody rigidbody;
    private ValueSmoothHandler<Vector3> vector3SmoothHandler;
    private ValueSmoothHandler<Quaternion> quaternionSmoothHandler;

    private const int ROTATIONSMOOTH_TASK_ID = 1;
    private const int POSITIONSMOOTH_TASK_ID = 2;
    private const int SCALESMOOTH_TASK_ID = 3;
    private const int VELOCITYSMOOTH_TASK_ID = 4;
    void Awake() {
        if(transform == null)
            Debug.LogError($"CharactorTransformController:{gameObject.name} transform hasn't been assigned");
        if(rigidbody == null)
            Debug.LogError($"CharactorTransformController:{gameObject.name} rigidbody hasn't been assigned");
        vector3SmoothHandler = new();
        quaternionSmoothHandler = new();
    }
}
