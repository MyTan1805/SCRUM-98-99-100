using System;
using System.Collections.Generic; // Cần cho List
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Is Victim List Not Empty",
    story: "Is the [VisibleVictims] list not empty?",
    category: "Firefighter/Conditional",
    id: "8063913c5270b388772928514075198a")] 
public partial class IsVictimListNotEmptyConditional : Action
{
    // --- ĐẦU VÀO TỪ BLACKBOARD ---
    [SerializeReference]
    [Tooltip("The list of visible victims to check.")]
    public BlackboardVariable<List<GameObject>> VisibleVictims;

    protected override Status OnStart()
    {
        if (VisibleVictims == null)
        {
            Debug.LogWarning("IsVictimListNotEmptyConditional: 'VisibleVictims' variable is not linked in the editor.");
            return Status.Failure;
        }

        var victimList = VisibleVictims.Value;

        if (victimList != null && victimList.Count > 0)
        {
            return Status.Success;
        }
        else
        {
            return Status.Failure;
        }
    }
}