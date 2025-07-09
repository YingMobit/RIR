using System;

/// <summary>
/// 伤害处理接口，用于伤害链式处理
/// </summary>
public interface IDamageProcessor {
    public IDamageProcessor next { get; set; }
    public void Process(ref DamageInfo damageInfo);

    public IDamageProcessor SetNext(IDamageProcessor damageProcessor);
}
