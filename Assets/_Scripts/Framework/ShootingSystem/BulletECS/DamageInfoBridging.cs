using Unity.Entities;
using UnityEngine;

public struct BulletDamageInfo {
    //适配实际系统使用ID代替GO
    public int FromEntityID;
    public float DamageValue;
    public float OriginDamgeValue;
    public float CriticalHitRate;
    public float CriticalDamageMultiper;
    public int DamageTags;
    public BulletDamageInfo(int fromID,float originDamageValue,int damageTags,float criticalHitRate = 0,float criticalDamageMultiper = 1) {
        FromEntityID = fromID;
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
                  $"From: {GameObjectEntityMappingSystem.Instance.FindGameObject(FromEntityID)?.name ?? "Unknown"}\n" +
                  $"Origin Damage: {OriginDamgeValue}\n" +
                  $"Final Damage: {DamageValue}\n" +
                  $"Critical Hit Rate: {CriticalHitRate * 100:F1}%\n" +
                  $"Critical Damage Multiplier: {CriticalDamageMultiper:F2}x\n" +
                  $"Damage Tags: {damageTagsText}\n" +
                  $"Damage Reduction: {damageReduction:F1}%\n" +
                  $"=========================");
    }
}

public static class DamageInfoBridge {
    public static DamageInfo ToDamageInfo(this BulletDamageInfo bulletDamageInfo) {
        return new(GameObjectEntityMappingSystem.Instance.FindGameObject(bulletDamageInfo.FromEntityID),
        bulletDamageInfo.OriginDamgeValue,
        bulletDamageInfo.DamageTags,
        bulletDamageInfo.CriticalHitRate,
        bulletDamageInfo.CriticalDamageMultiper);
    }

    public static BulletDamageInfo ToBulletDamageInfo(this DamageInfo damageInfo) {
        int FromEntityID = GameObjectEntityMappingSystem.Instance.FindEntityID(damageInfo.From);
        if(FromEntityID == 0) {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            FromEntityID = entityManager.CreateEntity().Index;
            GameObjectEntityMappingSystem.Instance.Regist(FromEntityID,damageInfo.From);
        }
        return new(FromEntityID,
        damageInfo.OriginDamgeValue,
        damageInfo.DamageTags,
        damageInfo.CriticalHitRate,
        damageInfo.CriticalDamageMultiper);
    }
}