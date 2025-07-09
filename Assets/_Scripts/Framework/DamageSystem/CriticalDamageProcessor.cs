using UnityEngine;
/// <summary>
/// ±©»÷ÉËº¦´¦Àí
/// </summary>
public class CriticalDamageProcessor : IDamageProcessor {
    public IDamageProcessor next { get; set; }

    public void Process(ref DamageInfo damageInfo) {
        Debug.Log("CriticalDamageProcessor.......");
        if(damageInfo.CriticalHitRate == 0) {
            next?.Process(ref damageInfo);
        } else {
            float chance = Random.Range(0,1);
            if(chance <= damageInfo.CriticalHitRate) {
                damageInfo.DamageValue *= damageInfo.CriticalDamageMultiper;
            }
            next?.Process(ref damageInfo);
        }

    }

    public IDamageProcessor SetNext(IDamageProcessor damageProcessor) {
        next = damageProcessor;
        return this;
    }
}
