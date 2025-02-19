using TMG.NFE_Tutorial;
using Unity.Entities;
using UnityEngine;

public class HitPointsAuthoring : MonoBehaviour
{
    public int MaxHitPoints;
    public Vector3 HealthBarOffset;

    public class HitPointsBaker: Baker<HitPointsAuthoring>
    {
        public override void Bake(HitPointsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CurrentHitPoints { Value = authoring.MaxHitPoints });
            AddComponent(entity, new MaxHitPoints{ Value = authoring.MaxHitPoints });
            AddBuffer<DamageBufferElement>(entity);
            AddBuffer<DamageThisTick>(entity);
            AddComponent(entity, new HealthBarOffset { Value = authoring.HealthBarOffset });
        }
    }
}
