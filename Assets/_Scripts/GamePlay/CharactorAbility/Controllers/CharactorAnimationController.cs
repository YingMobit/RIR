using GAS;
using PoolingSystem.ReferencePool;
using RollBackSystem;
using UnityEngine;
using Component = ECS.Component;

public class CharactorAnimationController : Component, IAnimationController {
    public ControllerTypeEnum Type => ControllerTypeEnum.Animation;

    #region IAnimationController
    Animator animator;
    AttributeSmoothHandler<float> floatAnimationParamSmoothHandler;

    public void SetBool(string name,bool value) {
        animator.SetBool(name,value);
    }

    public void SetFloat(string name,float value) {
        animator.SetFloat(name,value);
    }

    public void SetFloatSmooth(string name,float value,int smoothFrames) {
        int taskID = name.GetHashCode();

        floatAnimationParamSmoothHandler.RegisterTask(
            taskID,
            animator.GetFloat(name),
            value,
            smoothFrames,
            (v) => {
                animator.SetFloat(name,v);
            },
            (init,target,t) => { return Mathf.Lerp(init,target,t); },
            (a,b) => Mathf.Approximately(a,b)
        );
    }

    public void Update() {
        floatAnimationParamSmoothHandler.Update(Time.deltaTime);
    }

    public void LateUpdate() {

    }

    public void LogicUpdate() {

    }
    #endregion

    #region Component
    public override ECS.ComponentTypeEnum ComponentType => ECS.ComponentTypeEnum.CharactorAnimationControllerComponent;
    private GameObject gameObject;
    public GameObject GameObject => gameObject;

    public override void OnAttach(ECS.World world,ECS.Entity entity) {
        gameObject = world.GetGameObject(entity);
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    public override void Reset(ECS.World world,ECS.Entity entity) {
        gameObject = null;
        floatAnimationParamSmoothHandler.Reset();
    }

    public override Component GetNewInstance() {
        var res = new CharactorAnimationController();
        res.floatAnimationParamSmoothHandler = new AttributeSmoothHandler<float>();
        return res;
    }

    public override void OnDestroy() {
        floatAnimationParamSmoothHandler.Reset();
        gameObject = null;
        animator = null;
        floatAnimationParamSmoothHandler = null;
    }
    #endregion
}