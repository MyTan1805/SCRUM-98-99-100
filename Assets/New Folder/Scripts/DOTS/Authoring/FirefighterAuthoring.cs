using Unity.Entities;
using UnityEngine;
using Unity.Physics;

public class FirefighterAuthoring : MonoBehaviour
{
    public float DetectRange = 15f;
    public LayerMask VictimLayer;
    public LayerMask ObstacleLayer;

    class FirefighterBaker : Baker<FirefighterAuthoring>
    {
        public override void Bake(FirefighterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<FirefighterTag>(entity);
            AddComponent(entity, new DetectRange { Value = authoring.DetectRange });

            var victimFilter = new CollisionFilter
            {
                BelongsTo = ~0u, 
                CollidesWith = (uint)authoring.VictimLayer.value, 
                GroupIndex = 0
            };

            var obstacleFilter = new CollisionFilter
            {
                BelongsTo = ~0u, 
                CollidesWith = (uint)authoring.ObstacleLayer.value,
                GroupIndex = 0
            };

            AddComponent(entity, new VictimDetectionFilter
            {
                VictimFilter = victimFilter,
                ObstacleFilter = obstacleFilter
            });
            
            AddBuffer<VisibleVictim>(entity);
        }
    }
}