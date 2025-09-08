using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Scan For Victims",
    story: "Scan around [Agent] in [DetectRange] and update [VisibleVictims]",
    category: "Firefighter/Action",
    id: "b00e4119a6de52cacbed14574940b223")]
public partial class ScanForVictimsAction : Action
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Agent;

    [SerializeReference]
    public BlackboardVariable<float> DetectRange;

    [SerializeReference]
    [Tooltip("The name of the layer containing victims (e.g., 'Civilians'). Case-sensitive.")]
    public BlackboardVariable<string> VictimLayerName;
    
    [SerializeReference]
    [Tooltip("The name of the layer containing obstacles (e.g., 'Obstacles'). Case-sensitive.")]
    public BlackboardVariable<string> ObstacleLayerName;

    // --- ĐẦU RA VÀO BLACKBOARD ---
    [SerializeReference]
    [Tooltip("The Blackboard list to store found victims.")]
    public BlackboardVariable<List<GameObject>> VisibleVictims;


    protected override Node.Status OnStart()
    {
        var agentGo = Agent?.Value;
        var visibleVictimsList = VisibleVictims?.Value;
        var detectRangeValue = DetectRange?.Value ?? 0f;
        var victimLayerStr = VictimLayerName?.Value;
        var obstacleLayerStr = ObstacleLayerName?.Value;

        if (agentGo == null || VisibleVictims == null)
        {
            Debug.LogError("ScanForVictimsAction: 'Agent' or 'VisibleVictims' variable is not linked in the editor.", agentGo);
            return Status.Failure;
        }
        
        if (string.IsNullOrEmpty(victimLayerStr) || string.IsNullOrEmpty(obstacleLayerStr))
        {
            Debug.LogError("ScanForVictimsAction: 'VictimLayerName' or 'ObstacleLayerName' is not set on the Blackboard.", agentGo);
            return Status.Failure;
        }

        if (visibleVictimsList == null)
        {
            visibleVictimsList = new List<GameObject>();
            VisibleVictims.Value = visibleVictimsList;
        }
        else
        {
            visibleVictimsList.Clear();
        }

        int victimLayerMask = LayerMask.GetMask(victimLayerStr);
        int obstacleLayerMask = LayerMask.GetMask(obstacleLayerStr);

        
        Collider[] targetsInRadius = Physics.OverlapSphere(agentGo.transform.position, detectRangeValue, victimLayerMask);
        
        foreach (var targetCollider in targetsInRadius)
        {
            Transform targetTransform = targetCollider.transform;
            
            Vector3 raycastOrigin = agentGo.transform.position + Vector3.up * 1.0f;
            
            Vector3 directionToTarget = targetTransform.position - raycastOrigin;
            float distanceToTarget = directionToTarget.magnitude;

            bool hitObstacle = Physics.Raycast(raycastOrigin, directionToTarget.normalized, distanceToTarget, obstacleLayerMask);
            
            if (hitObstacle)
            {
                Debug.DrawRay(raycastOrigin, directionToTarget.normalized * distanceToTarget, Color.red, 1.0f);
            }
            else
            {
                Debug.DrawRay(raycastOrigin, directionToTarget.normalized * distanceToTarget, Color.green, 1.0f);
            }
            
            if (!hitObstacle)
            {
                visibleVictimsList.Add(targetTransform.gameObject);
            }
        }
        
        Debug.Log($"Scan complete for {agentGo.name}. Found {visibleVictimsList.Count} visible victims.");

        return Status.Success;
    }
}