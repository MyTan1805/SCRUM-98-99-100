using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ScanForVictimsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FirefighterTag>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var victimTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true);

        foreach (var (transform, detectRange, filter, visibleVictims)
                 in SystemAPI.Query<RefRO<LocalTransform>, RefRO<DetectRange>, RefRO<VictimDetectionFilter>, DynamicBuffer<VisibleVictim>>().WithAll<FirefighterTag>())
        {
            visibleVictims.Clear();
            var firefighterPosition = transform.ValueRO.Position;
            var range = detectRange.ValueRO.Value;

            // Dùng NativeList<DistanceHit> vì OverlapSphere trả về kiểu này
            var hits = new NativeList<DistanceHit>(Allocator.Temp);

            // Dùng OverlapSphere là đủ và chính xác cho việc tìm các đối tượng trong bán kính
            if (physicsWorld.OverlapSphere(firefighterPosition, range, ref hits, filter.ValueRO.VictimFilter))
            {
                foreach (var hit in hits)
                {
                    Entity victimEntity = hit.Entity;
                    var victimPosition = victimTransforms[victimEntity].Position;
                    var directionToVictim = victimPosition - firefighterPosition;
                    var distanceToVictim = math.length(directionToVictim);

                    var rayInput = new RaycastInput
                    {
                        Start = firefighterPosition,
                        End = victimPosition,
                        Filter = filter.ValueRO.ObstacleFilter
                    };

                    // Kiểm tra vật cản
                    if (physicsWorld.CastRay(rayInput, out var rayHit))
                    {
                        // Có vật cản, vẽ đường màu đỏ
                        Debug.DrawLine(firefighterPosition, rayHit.Position, Color.red);
                    }
                    else
                    {
                        // Không có vật cản, thêm vào danh sách và vẽ đường màu xanh
                        Debug.DrawLine(firefighterPosition, victimPosition, Color.green);
                        visibleVictims.Add(new VisibleVictim
                        {
                            VictimEntity = victimEntity,
                            Distance = distanceToVictim
                        });
                    }
                }
            }
        }
    }
}