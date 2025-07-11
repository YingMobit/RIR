using System;
public abstract class AbilityModifier<TValue> {
    public abstract void Modify(ref TValue oriValue);
}

public class FloatAddAbilityModifier : AbilityModifier<float> {
    private float add;
    public override void Modify(ref float oriValue) {
        oriValue += add;
    }

    public FloatAddAbilityModifier(float addin) {
        add = addin;
    }
}

public class IntAddAbilityModifier : AbilityModifier<int> {
    private int add;
    public override void Modify(ref int oriValue) {
        oriValue += add;
    }
    public IntAddAbilityModifier(int addin) {
        add = addin;
    }
}

public class MultiperAbilityModifier : AbilityModifier<float> {
    private float multiper;

    public override void Modify(ref float oriValue) {
        oriValue *= multiper;
    }

    public MultiperAbilityModifier(float _multiper) {
        multiper = _multiper;
    }
}