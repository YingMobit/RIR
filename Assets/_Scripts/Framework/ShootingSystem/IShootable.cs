using UnityEngine;
/// <summary>
/// �������ӿ�
/// </summary>
public interface IShootable {
    public Transform SpawnPoint { get; }
    public ShootableType shootableType { get; }
    public void Shoot();
}

/// <summary>
/// ����������
/// </summary>
public enum ShootableType {

}