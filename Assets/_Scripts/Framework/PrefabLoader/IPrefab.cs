using System;
using UnityEngine;

/// <summary>
/// Ԥ����ӿ�
/// </summary>
/// <typeparam name="PrefabType">����Ψһ��ʶ���Ԥ���������</typeparam>
public interface IPrefab<PrefabType> : IPrefab {
    public string TagName { get => "Defualt"; }
    public PrefabType Copy();
}

public interface IPrefab {
    public Type Type { get; }
}
