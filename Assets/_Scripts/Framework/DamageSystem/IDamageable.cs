using UnityEngine;
public interface IDamageable {
    public IDamageProcessor DamageProcessors { get; set; }
    public void TakeDamage(DamageInfo damageInfo);
}

public struct DamageInfo {
    public GameObject From;
    public GameObject To;
    public float DamageValue;
    public float OriginDamgeValue;
    public float CriticalHitRate;
    public float CriticalDamageMultiper;
    public DamageTags[] DamageTags;
    public DamageInfo(GameObject from,GameObject to,float originDamageValue,float criticalHitRate = 0,float criticalDamageMultiper = 1,DamageTags[] damageTags = null) {
        From = from;
        To = to;
        DamageValue = originDamageValue;
        OriginDamgeValue = originDamageValue;
        CriticalHitRate = criticalHitRate;
        CriticalDamageMultiper = criticalDamageMultiper;
        DamageTags = damageTags;
    }

    public void DebugDamageInfo() {
        float damageReduction = ((OriginDamgeValue - DamageValue) / OriginDamgeValue) * 100;
        string damageTagsText = (DamageTags != null && DamageTags.Length > 0) ?
            string.Join(", ",DamageTags) : "None";

        Debug.Log($"=== Damage Info Debug ===\n" +
                  $"From: {(From != null ? From.name : "Unknown")}\n" +
                  $"To: {(To != null ? To.name : "Unknown")}\n" +
                  $"Origin Damage: {OriginDamgeValue}\n" +
                  $"Final Damage: {DamageValue}\n" +
                  $"Critical Hit Rate: {CriticalHitRate * 100:F1}%\n" +
                  $"Critical Damage Multiplier: {CriticalDamageMultiper:F2}x\n" +
                  $"Damage Tags: {damageTagsText}\n" +
                  $"Damage Reduction: {damageReduction:F1}%\n" +
                  $"=========================");
    }
}

public enum DamageTags {

}
