using UnityEngine;
using GAS;
using ReferencePoolingSystem;

public class CharactorAnimationController : IAnimationController , IReference<CharactorAnimationController> {
    public ControllerTypeEnum Type => ControllerTypeEnum.Animation;

    Animator animator;
    ValueSmoothHandler<float> floatAnimationParamSmoothHandler;

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

    public void Update() { 
        floatAnimationParamSmoothHandler.Update();
    }

    int IReference.IndexInRefrencePool { get; set; }
    public uint ReferenceType => ReferenceTypes.CHARACTORANIMATIONCONTROLLER;
    public GameObject GameObject => gameObject;
    private GameObject gameObject;

    public void BindGameObject(GameObject gameObject) {
        this.gameObject = gameObject;
        animator = gameObject.GetComponentInChildren<Animator>();
    }


    public void LateUpdate() {
        
    }

    public void OnRecycle() {
        gameObject = null;
        floatAnimationParamSmoothHandler.Reset();
    }

    public IReference Clone() {
        var res = new CharactorAnimationController();
        res.floatAnimationParamSmoothHandler = new ValueSmoothHandler<float>();
        return res;
    }

    public void Dispose() {
        OnRecycle();
        gameObject = null;
        animator = null;
        floatAnimationParamSmoothHandler = null;
    }
}