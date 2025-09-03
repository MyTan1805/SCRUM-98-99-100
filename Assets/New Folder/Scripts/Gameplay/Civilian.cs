using UnityEngine;
using UnityEngine.AI;

public class Civilian : MonoBehaviour, ITargetable
{
    public enum CivilianState { Panicked, Unconscious, Following, Rescued }

    [Header("State")]
    [SerializeField] private CivilianState currentState = CivilianState.Panicked;
    [Tooltip("Đã được bot kiểm tra xong để tránh chọn lại.")]
    public bool assessed = false;

    [Header("Interact")]
    [Tooltip("Điểm bot quỳ/kiểm tra. Trống sẽ dùng vị trí nạn nhân.")]
    public Transform interactPoint;

    NavMeshAgent agent;
    Transform rescuer;

    // Helpers
    public bool IsUnconscious => currentState == CivilianState.Unconscious;
    public bool IsAvailable =>
    !assessed && (currentState == CivilianState.Panicked || currentState == CivilianState.Unconscious);
    public Vector3 GetInteractPos() => interactPoint ? interactPoint.position : transform.position;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!agent) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.enabled = false;
    }

    void Update()
    {
        if (currentState == CivilianState.Following && rescuer != null && agent.enabled)
        {
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
                    agent.Warp(hit.position);
            }
            agent.SetDestination(rescuer.position);
        }
    }

    // “Khám nhanh” để xuất trạng thái
    public string Assess()
    {
        assessed = true;
        return IsUnconscious ? "Bất tỉnh" : "Còn tỉnh táo";
    }

    // Bắt đầu cứu (cho nạn nhân theo sau rescuer)
    public void StartRescue(Transform rescuerTransform)
    {
        if (currentState == CivilianState.Rescued) return;

        rescuer = rescuerTransform;

        if (!agent.enabled) agent.enabled = true;
        if (!agent.isOnNavMesh && NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        currentState = CivilianState.Following;
        Debug.Log($"{name} is following {rescuerTransform.name}");
    }

    // Hoàn tất cứu
    public void CompleteRescue()
    {
        currentState = CivilianState.Rescued;
        if (agent) agent.enabled = false;
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        var p = GetInteractPos();
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(p, 0.06f);
        Gizmos.DrawLine(transform.position, p);
    }

    public Vector3 GetPosition() => transform.position;
    public TargetType GetTargetType() => TargetType.Civilian;
    public void Interact(IAgent agentWho)
    {
        if (currentState == CivilianState.Rescued) return;
        StartRescue(agentWho.transform);
    }
    public bool IsTaskComplete() =>
        currentState == CivilianState.Following || currentState == CivilianState.Rescued;

    public void Interact(FirefighterBot bot)
    {
        throw new System.NotImplementedException();
    }
}
