using System;

/// <summary>
/// �˺�����ӿڣ������˺���ʽ����
/// </summary>
public interface IDamageProcessor {
    public IDamageProcessor next { get; set; }
    public void Process(ref DamageInfo damageInfo);

    public IDamageProcessor SetNext(IDamageProcessor damageProcessor);
}
