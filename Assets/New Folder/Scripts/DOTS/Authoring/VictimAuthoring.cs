using Unity.Entities;
using UnityEngine;

public class VictimAuthoring : MonoBehaviour
{
    class VictimBaker : Baker<VictimAuthoring>
    {
        public override void Bake(VictimAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<VictimTag>(entity);
        }
    }
}