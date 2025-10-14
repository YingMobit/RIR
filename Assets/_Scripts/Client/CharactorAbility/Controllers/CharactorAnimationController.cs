using UnityEngine;
using GAS;

public class CharactorAnimationController : MonoBehaviour, IAnimationController {
    public ControllerTypeEnum Type => ControllerTypeEnum.Animation;

    [SerializeField] Animator animator;
    ValueSmoothHandler<float> floatAnimationParamSmoothHandler;
    void Awake() {
        if(animator == null)
            Debug.LogError($"CharactorAnimationController:{gameObject.name}: animator hasn't been assigned!");
        floatAnimationParamSmoothHandler = new();
    }

    public void SetBool(string name,bool value) {
        animator.SetBool(name,value);
    }

    public void SetFloat(string name,float value) {
        animator.SetFloat(name,value);
    }

    public void SetFloatSmooth(string name,float value,float smoothTime) {
        floatAnimationParamSmoothHandler.RegistTask(name.GetHashCode(),animator.GetFloat(name),value,smoothTime,(v)=> {
            animator.SetFloat(name,v);
        },(init,target,t) => { 
            return Mathf.Lerp(init,target,t);
        });
    }

    void Update() { 
        floatAnimationParamSmoothHandler.Update();
    }
}