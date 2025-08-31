using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// EntityMono桥接类，基础的MonoEntity模板,默认Entity具有位置和碰撞
/// </summary>
public class EntitiedMonoBehavior : MonoBehaviour {
    protected Entity m_VirtualEntity;
    public Entity Entity => m_VirtualEntity;
    protected EntityManager entityManager;
    private bool m_Valid;
    public bool Valid => m_Valid;

    protected virtual void Awake() {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        InitiateEntity();
        if(m_VirtualEntity != Entity.Null && entityManager.Exists(m_VirtualEntity)) {
            GameObjectEntityMappingSystem.Instance.Regist(m_VirtualEntity,gameObject);
            m_Valid = true;
        }
    }

    protected virtual void InitiateEntity() {
        m_VirtualEntity = entityManager.CreateEntity();

        // 1. 设置位置（碰撞检测必需）
        entityManager.AddComponentData(m_VirtualEntity,LocalTransform.FromMatrix(
            Matrix4x4.TRS(transform.position,transform.rotation,transform.localScale)
        ));


        // 2. 创建物理碰撞体（碰撞检测必需）
        var physicsCollider = CreatePhysicsCollider();
        if(physicsCollider.IsCreated) {
            entityManager.AddComponentData(m_VirtualEntity,new PhysicsCollider {
                Value = physicsCollider
            });
        }

        // 3. 添加最基本的物理组件（Unity Physics 要求）
        entityManager.AddComponentData(m_VirtualEntity,PhysicsMass.CreateKinematic(MassProperties.UnitSphere));
        entityManager.AddComponentData(m_VirtualEntity,PhysicsVelocity.Zero);
    }

    protected virtual void Update() {
        UpdateEntity();
    }

    protected virtual void UpdateEntity() {
        if(!m_Valid)
            return;

        // 只同步位置信息（影响碰撞检测位置）
        entityManager.SetComponentData(m_VirtualEntity,LocalTransform.FromMatrix(
            Matrix4x4.TRS(transform.position,transform.rotation,transform.localScale)
        ));
    }

    // 根据 GameObject 的 Collider 创建对应的 Physics Collider
    protected virtual BlobAssetReference<Unity.Physics.Collider> CreatePhysicsCollider() {
        var collider = GetComponent<UnityEngine.Collider>();

        if(collider == null) {
            // 默认球形碰撞体
            return Unity.Physics.SphereCollider.Create(new SphereGeometry {
                Center = float3.zero,
                Radius = 0.5f
            });
        }

        switch(collider) {
            case UnityEngine.SphereCollider sphere:
                return Unity.Physics.SphereCollider.Create(new SphereGeometry {
                    Center = sphere.center,
                    Radius = sphere.radius
                });

            case UnityEngine.BoxCollider box:
                return Unity.Physics.BoxCollider.Create(new BoxGeometry {
                    Center = box.center,
                    Orientation = quaternion.identity,
                    Size = box.size,
                    BevelRadius = 0.05f
                });

            case UnityEngine.CapsuleCollider capsule:
                return Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry {
                    Vertex0 = (float3)capsule.center + new float3(0,-capsule.height * 0.5f + capsule.radius,0),
                    Vertex1 = (float3)capsule.center + new float3(0,capsule.height * 0.5f - capsule.radius,0),
                    Radius = capsule.radius
                });

            default:
                // 不支持的类型，使用包围盒
                var bounds = collider.bounds;
                return Unity.Physics.BoxCollider.Create(new BoxGeometry {
                    Center = float3.zero,
                    Orientation = quaternion.identity,
                    Size = bounds.size,
                    BevelRadius = 0.05f
                });
        }
    }

    protected virtual void OnDestroy() {
        if(m_Valid) {
            GameObjectEntityMappingSystem.Instance.UnRegist(gameObject);

            // 清理 Physics Collider 资源
            if(entityManager.HasComponent<PhysicsCollider>(m_VirtualEntity)) {
                var physicsCollider = entityManager.GetComponentData<PhysicsCollider>(m_VirtualEntity);
                if(physicsCollider.Value.IsCreated) {
                    physicsCollider.Value.Dispose();
                }
            }

            entityManager.DestroyEntity(m_VirtualEntity);
            m_Valid = false;
        }
    }
}