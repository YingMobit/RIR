using System;
using System.Collections.Generic;
using UnityEngine;
public interface IDamageable {
    public IDamageProcessor DamageProcessors { get; set; }
    public void TakeDamage(DamageInfo damageInfo);
}

[Serializable]
public struct DamageInfo {
    public GameObject From;
    public float DamageValue;
    public float OriginDamgeValue;
    public float CriticalHitRate;
    public float CriticalDamageMultiper;
    public int DamageTags;
    public DamageInfo(GameObject from,float originDamageValue,int damageTags,float criticalHitRate = 0,float criticalDamageMultiper = 1) {
        From = from;
        DamageValue = originDamageValue;
        OriginDamgeValue = originDamageValue;
        CriticalHitRate = criticalHitRate;
        CriticalDamageMultiper = criticalDamageMultiper;
        DamageTags = damageTags;
    }

    public void DebugDamageInfo() {
        float damageReduction = ((OriginDamgeValue - DamageValue) / OriginDamgeValue) * 100;

        var damageTagsList = DamageTags.GetDamageTags();
        string damageTagsText = (damageTagsList != null && damageTagsList.Count > 0) ?
            string.Join(", ",damageTagsList) : "None";


        Debug.Log($"=== Damage Info Debug ===\n" +
                  $"From: {From?.name ?? "Unknown"}\n" +
                  $"Origin Damage: {OriginDamgeValue}\n" +
                  $"Final Damage: {DamageValue}\n" +
                  $"Critical Hit Rate: {CriticalHitRate * 100:F1}%\n" +
                  $"Critical Damage Multiplier: {CriticalDamageMultiper:F2}x\n" +
                  $"Damage Tags: {damageTagsText}\n" +
                  $"Damage Reduction: {damageReduction:F1}%\n" +
                  $"=========================");
    }
}