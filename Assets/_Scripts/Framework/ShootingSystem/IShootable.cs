using UnityEngine;
/// <summary>
/// 发射器接口
/// </summary>
public interface IShootable {
    public Transform SpawnPoint { get; }
    public ShootableType shootableType { get; }
    public void Shoot();
}

/// <summary>
/// 发射器类型
/// </summary>
public enum ShootableType {

}