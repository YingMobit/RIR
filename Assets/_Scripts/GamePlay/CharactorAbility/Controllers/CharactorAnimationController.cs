using GAS;
using PoolingSystem.ReferencePool;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class CharactorAnimationController : IAnimationController , IReference<CharactorAnimationController> {
    public ControllerTypeEnum Type => ControllerTypeEnum.Animation;

    Animator animator;
    AttributeSmoothHandler<float> floatAnimationParamSmoothHandler;

    public void SetBool(string name,bool value) {
        animator.SetBool(name,value);
    }

    public void SetFloat(string name,float value) {
        animator.SetFloat(name,value);
    }

    public void SetFloatSmooth(string name, float value, int smoothFrames) {
        int taskID = name.GetHashCode();

        floatAnimationParamSmoothHandler.RegistTask(
            taskID,
            animator.GetFloat(name),
            value,
            smoothFrames,
            (v) => {
                Debug.Log($"[CharactorAnimationController]:Set AnimationParame:{name},value:{v}");
                animator.SetFloat(name,v);
            },
            (init,target,t) => { Debug.Log($"[CharactorAnimationController]:Lerp AnimationParame:{name},t: {t},init: {init},taget: {target},value: {Mathf.Lerp(init,target,t)}"); return Mathf.Lerp(init,target,t); },
            (a,b) => Mathf.Approximately(a,b)
        );
    }

    public void Update() { 
        floatAnimationParamSmoothHandler.Update(Time.deltaTime);
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
    
    public void LogicUpdate() {
        
    }

    public void OnRecycle() {
        gameObject = null;
        floatAnimationParamSmoothHandler.Reset();
    }

    public IReference GetNewInstance() {
        var res = new CharactorAnimationController();
        res.floatAnimationParamSmoothHandler = new AttributeSmoothHandler<float>();
        return res;
    }

    public void Dispose() {
        OnRecycle();
        gameObject = null;
        animator = null;
        floatAnimationParamSmoothHandler = null;
    }
}