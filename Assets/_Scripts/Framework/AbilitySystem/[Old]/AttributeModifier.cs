using System;
public abstract class AttributeModifier<TValue> {
    public abstract void Modify(ref TValue oriValue);
}

public class FloatAddAttributeModifier : AttributeModifier<float> {
    private float add;
    public override void Modify(ref float oriValue) {
        oriValue += add;
    }

    public FloatAddAttributeModifier(float addin) {
        add = addin;
    }
}

public class IntAddAttributeModifier : AttributeModifier<int> {
    private int add;
    public override void Modify(ref int oriValue) {
        oriValue += add;
    }
    public IntAddAttributeModifier(int addin) {
        add = addin;
    }
}

public class MultiperAttributeModifier : AttributeModifier<float> {
    private float multiper;

    public override void Modify(ref float oriValue) {
        oriValue *= multiper;
    }

    public MultiperAttributeModifier(float _multiper) {
        multiper = _multiper;
    }
}