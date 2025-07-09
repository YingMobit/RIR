using UnityEngine;

/// <summary>
/// …À∫¶∏°∂Ø¥¶¿Ì
/// </summary>
public class FloatingDamageProcessor : IDamageProcessor {
    private float floatingRate;

    public IDamageProcessor next { get; set; }

    public void Process(ref DamageInfo damageInfo) {
        Debug.Log("FloatingDamageProcessor......");
        float floating = Random.Range(-floatingRate,floatingRate);
        damageInfo.DamageValue *= 1 + floating;

        next?.Process(ref damageInfo);
    }

    public IDamageProcessor SetNext(IDamageProcessor damageProcessor) {
        next = damageProcessor;
        return this;
    }

    public FloatingDamageProcessor(float _floatingRate = 0.1f) {
        floatingRate = _floatingRate;
    }
}