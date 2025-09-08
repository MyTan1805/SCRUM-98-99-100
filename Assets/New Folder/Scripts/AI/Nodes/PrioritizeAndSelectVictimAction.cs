using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Prioritize And Select Victim",
    story: "Select best victim from [VisibleVictims] and set to [CurrentTarget]",
    category: "Firefighter/Action",
    id: "YOUR_UNIQUE_ID_HERE")]
public partial class PrioritizeAndSelectVictimAction : Action
{
    // --- ĐẦU VÀO TỪ BLACKBOARD ---
    [SerializeReference]
    public BlackboardVariable<GameObject> Agent;

    [SerializeReference]
    public BlackboardVariable<List<GameObject>> VisibleVictims;

    // --- ĐẦU RA VÀO BLACKBOARD (Tạm thời không dùng) ---
    [SerializeReference]
    public BlackboardVariable<GameObject> CurrentTarget;


    protected override Status OnStart()
    {
        var agentGo = Agent?.Value;
        var visibleVictimsList = VisibleVictims?.Value;

        if (agentGo == null || visibleVictimsList == null)
        {
            Debug.LogError("PrioritizeAndSelectVictimAction: Agent or VisibleVictims variable is not set/linked.", agentGo);
            return Status.Failure;
        }

        if (visibleVictimsList.Count == 0)
        {
            Debug.Log("PrioritizeAndSelectVictimAction: Victim list is empty. Nothing to do.");
            return Status.Failure; 
        }
        
        // --- LOGIC TÍNH TOÁN KHOẢNG CÁCH VÀ DEBUG ---
        
        Debug.Log("--- Prioritizing Victims ---");
        
        foreach (var victimGo in visibleVictimsList)
        {
            if(victimGo == null) continue;

            float distance = Vector3.Distance(agentGo.transform.position, victimGo.transform.position);

            Debug.Log($"Victim: {victimGo.name}, Distance: {distance:F2} meters");
        }
        
        /*
        // ===================================================================
        // === PHẦN LỰA CHỌN VÀ GÁN MỤC TIÊU ĐÃ ĐƯỢC VÔ HIỆU HÓA (COMMENTED OUT) ===
        // ===================================================================

        GameObject bestTarget = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (var victimGo in visibleVictimsList)
        {
            if(victimGo == null) continue;
            float distanceSqr = (agentGo.transform.position - victimGo.transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                bestTarget = victimGo;
            }
        }
        
        if (bestTarget != null)
        {
            Debug.Log($"{agentGo.name} would have selected target: {bestTarget.name}");
            // CurrentTarget.Value = bestTarget; // DÒNG NÀY ĐÃ BỊ VÔ HIỆU HÓA
        }
        */


        // Vì chúng ta chỉ đang debug, hãy trả về Success để cho thấy node đã chạy xong phần tính toán.
        return Status.Success;
    }
}