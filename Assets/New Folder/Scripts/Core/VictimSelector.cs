using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CivilianSelector
{
    // Chọn nạn nhân tốt nhất từ list đã có
    public static Civilian SelectBest(IList<Civilian> list, Vector3 rescuerPos)
    {
        if (list == null || list.Count == 0) return null;

        return list
            .Where(c => c != null && c.IsAvailable) // chỉ Panicked/Unconscious, chưa assessed
            .OrderByDescending(c => c.IsUnconscious) // bất tỉnh trước
            .ThenBy(c => (c.GetInteractPos() - rescuerPos).sqrMagnitude) // rồi gần nhất
            .FirstOrDefault();
    }

    // Tiện dụng: chọn trực tiếp từ mảng Collider khi dùng Physics.OverlapSphere
    public static Civilian SelectBest(Collider[] hits, Vector3 rescuerPos)
    {
        if (hits == null || hits.Length == 0) return null;

        return hits
            .Select(h => h.GetComponentInParent<Civilian>())
            .Where(c => c != null && c.IsAvailable)
            .OrderByDescending(c => c.IsUnconscious)
            .ThenBy(c => (c.GetInteractPos() - rescuerPos).sqrMagnitude)
            .FirstOrDefault();
    }
}
