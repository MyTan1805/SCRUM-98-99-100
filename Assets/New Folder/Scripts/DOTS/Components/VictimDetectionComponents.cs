using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

// === TAGS ===
public struct FirefighterTag : IComponentData { }
public struct VictimTag : IComponentData { }

// === COMPONENT DATA ===
public struct DetectRange : IComponentData
{
    public float Value;
}

public struct VictimDetectionFilter : IComponentData
{
    public CollisionFilter VictimFilter;
    public CollisionFilter ObstacleFilter;
}

// === DYNAMIC BUFFER ===
[InternalBufferCapacity(8)] // Cấp phát sẵn không gian cho 8 nạn nhân
public struct VisibleVictim : IBufferElementData
{
    public Entity VictimEntity;
    public float Distance;
}