using UnityEngine;
using UnityEngine.AI; 

[RequireComponent(typeof(Collider), typeof(NavMeshAgent))]
public class Civilian : MonoBehaviour, ITargetable
{
    public enum CivilianState { Panicked, Unconscious, Following, Rescued }

    [Header("Civilian State")]
    [SerializeField] private CivilianState currentState = CivilianState.Panicked;

    private NavMeshAgent navMeshAgent;
    private Transform rescuer = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.enabled = false; 
    }

    private void Update()
    {
        if (currentState == CivilianState.Following && rescuer != null)
        {
            navMeshAgent.SetDestination(rescuer.position);
        }
    }

    public void StartRescue(Transform rescuerTransform)
    {
        if (currentState == CivilianState.Rescued) return;

        Debug.Log(gameObject.name + " is being rescued by " + rescuerTransform.name);
        currentState = CivilianState.Following;
        rescuer = rescuerTransform;
        navMeshAgent.enabled = true; 
    }

    public void CompleteRescue()
    {
        currentState = CivilianState.Rescued;
        navMeshAgent.enabled = false;
        gameObject.SetActive(false);
    }


    #region ITargetable Implementation

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public TargetType GetTargetType()
    {
        return TargetType.Civilian;
    }

    public void Interact(IAgent agent)
    {
        if (currentState != CivilianState.Following && currentState != CivilianState.Rescued)
        {
            StartRescue(agent.transform);
        }
    }

    public bool IsTaskComplete()
    {
        return currentState == CivilianState.Following || currentState == CivilianState.Rescued;
    }

    public void Interact(FirefighterBot bot)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}