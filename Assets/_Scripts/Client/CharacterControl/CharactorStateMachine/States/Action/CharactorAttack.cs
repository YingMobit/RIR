using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorAttack",menuName = CharactorFSMStateDataConfig.StateDataMenuPathRoot + "CharactorAttack",order = 0)]
public class CharactorAttack : CharactorStateBase {
    public override IStateMachine stateMachine { get; set; }

    [SerializeField] private float AttackInterval;
    private float LastAttackTime = -10;
    private IShootable shootable;

    public override CharactorStateBase Clone() {
        return Instantiate(this);
    }

    public void Init(IShootable _shootable) {
        shootable = _shootable;
    }

    public override void OnEnter() {

    }

    public override void OnExit() {

    }

    public override void OnFixedUpdate() {

    }

    public override void OnUpdate() {
        if(Time.time - LastAttackTime > AttackInterval) {
            shootable.Shoot();
            LastAttackTime = Time.time;
        }
    }
}
