using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AutoRescueController : MonoBehaviour
{
    [Header("Vision")]
    public float senseRadius = 25f;
    [Range(1f, 360f)] public float fovAngle = 360f;          // 360 = bỏ qua FOV
    public LayerMask victimMask;                              // Layer của Victim/Civilian
    public LayerMask obstacleMask;                            // CHỈ layer vật cản (tường/hộp). Để Nothing nếu muốn bỏ LOS
    public float eyeHeight = 1.6f;

    [Header("Interact")]
    public float interactRange = 1.6f;

    [Header("Animation")]
    public Animator animator;                                 // nếu để trống sẽ tự tìm
    public string kneelTrigger = "KneelDown";
    public string standTrigger = "StandUp";
    public string locomotionSpeedParam = "Velocity";          // để trống nếu không dùng float tốc độ

    [Header("Durations (sec)")]
    public float kneelDuration = 0.8f;
    public float checkDuration = 1.2f;
    public float standDuration = 0.7f;

    [Header("Approach Slowdown")]
    public bool useRaySlowdown = true;   // dùng ray để xác nhận phía trước có nạn nhân
    public float slowDownStartDist = 2.5f;   // bắt đầu giảm tốc khi còn cách khoảng này
    public float minApproachSpeed = 0.2f;   // tốc độ tối thiểu khi sắp đến nơi
    public float slowRayRadius = 0.3f;   // bán kính spherecast
    public float speedLerp = 6f;
    float baseSpeed;

    // runtime
    NavMeshAgent agent;
    Civilian target;

    // animator safety
    int speedHash = -1;
    int kneelHash = -1;
    int standHash = -1;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();


        agent.updatePosition = agent.updateRotation = true;
        baseSpeed = agent.speed;
        agent.autoBraking = true;
        agent.stoppingDistance = interactRange;
        agent.updateRotation = true;

        // khóa tham số Animator (tránh lỗi "Parameter 'X' does not exist")
        if (animator)
        {
            foreach (var p in animator.parameters)
            {
                if (!string.IsNullOrEmpty(locomotionSpeedParam) &&
                    p.type == AnimatorControllerParameterType.Float &&
                    p.name == locomotionSpeedParam)
                    speedHash = Animator.StringToHash(p.name);

                if (p.type == AnimatorControllerParameterType.Trigger && p.name == kneelTrigger)
                    kneelHash = Animator.StringToHash(p.name);

                if (p.type == AnimatorControllerParameterType.Trigger && p.name == standTrigger)
                    standHash = Animator.StringToHash(p.name);
            }
            if (!string.IsNullOrEmpty(locomotionSpeedParam) && speedHash == -1)
                locomotionSpeedParam = null; // bỏ cập nhật tốc độ nếu animator không có param này
        }
    }

    void OnEnable() { StartCoroutine(RescueLoop()); }
    void OnDisable() { StopAllCoroutines(); }

    void Update()
    {
        if (animator != null && speedHash != -1)
            animator.SetFloat(speedHash, agent.velocity.magnitude);
    }

    IEnumerator RescueLoop()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"[{name}] NavMeshAgent is not on NavMesh.");
            yield break;
        }

        while (true)
        {
            // 1) tìm mục tiêu
            target = FindBestVictim();
            if (target == null) { yield return null; continue; }

            // 2) tiếp cận
            Vector3 ipos = GetSafeNavmeshPoint(target.GetInteractPos(), 1.5f);
            agent.isStopped = false;
            agent.SetDestination(ipos);

            while (agent.pathPending) yield return null;
            // Thêm kiểm tra path
            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                target = null;
                yield return null;
                continue;
            }

            // 3) tiến đến khi vào tầm
            float interactSqr = interactRange * interactRange;
            while (target != null && target.IsAvailable &&
                (ipos - transform.position).sqrMagnitude > interactSqr)
            {
                UpdateApproachSpeed(ipos);
                Vector3 newIPos = GetSafeNavmeshPoint(target.GetInteractPos(), 1.5f);
                if (!agent.pathPending && (newIPos - ipos).sqrMagnitude > 0.04f)
                {
                    ipos = newIPos;
                    agent.SetDestination(ipos);
                }
                yield return null;
            }

            if (target == null || !target.IsAvailable) { yield return null; continue; }

            // 4) đến nơi: dừng, xoay, quỳ
            agent.isStopped = true;
            agent.ResetPath();
            Face(ipos);

            // chờ thật sự dừng (Velocity ~ 0) để điều kiện transition thỏa
            while (agent.velocity.sqrMagnitude > 0.0004f)
            {
                if (animator && speedHash != -1) animator.SetFloat(speedHash, 0f); // ép tham số về 0
                yield return null;
            }
            // thêm 1 frame để Animator cập nhật
            yield return null;

            // tránh xung đột trigger ngược
            if (animator && standHash != -1) animator.ResetTrigger(standHash);

            // bắn trigger quỳ
            if (animator && kneelHash != -1) animator.SetTrigger(kneelHash);
            yield return new WaitForSeconds(kneelDuration);

            // 5) kiểm tra nhanh
            yield return new WaitForSeconds(checkDuration);

            // 6) kết quả & đánh dấu
            string result = target.Assess();
            Debug.Log($"[Assessment] {target.name}: {result}");

            // 7) đứng dậy
            if (animator && standHash != -1) animator.SetTrigger(standHash);
            yield return new WaitForSeconds(standDuration);

            // 8) vòng mới
            target = null;
            agent.isStopped = false;
            yield return null;
        }
    }


    Civilian FindBestVictim()
    {
        // Nếu quên gán mask trên Inspector thì coi như Everything
        LayerMask vm = (victimMask.value == 0) ? ~0 : victimMask;

        var cols = Physics.OverlapSphere(transform.position, senseRadius, vm);
        if (cols == null || cols.Length == 0) return null;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;

        // Lọc các Civilian còn khả dụng + trong FOV + không bị che LOS
        var candidates = cols
            .Select(c => c.GetComponentInParent<Civilian>())
            .Where(v => v != null && v.IsAvailable)
            .Where(v =>
            {
                Vector3 to = v.GetInteractPos() - eye;
                float dist = to.magnitude;
                if (dist < 0.01f) return false;

                Vector3 dir = to / dist;

                // FOV (bỏ qua nếu fovAngle = 360)
                if (fovAngle < 360f && Vector3.Angle(transform.forward, dir) > fovAngle * 0.5f)
                    return false;

                // LOS chỉ check obstacleMask (không gồm Civilian/Ground)
                if (obstacleMask.value != 0 &&
                    Physics.Raycast(eye + dir * 0.05f, dir, dist - 0.05f, obstacleMask, QueryTriggerInteraction.Ignore))
                    return false;

                return true;
            });

        // Ưu tiên: Unconscious trước, rồi tới gần nhất
        return candidates
            .OrderByDescending(v => v.IsUnconscious) // true -> đứng trước
            .ThenBy(v => (v.GetInteractPos() - transform.position).sqrMagnitude)
            .FirstOrDefault();
    }

    Vector3 GetSafeNavmeshPoint(Vector3 src, float maxSnapDist)
    {
        return NavMesh.SamplePosition(src, out var hit, maxSnapDist, NavMesh.AllAreas) ? hit.position : src;
    }

    void Face(Vector3 worldPos)
    {
        Vector3 d = worldPos - transform.position; d.y = 0f;
        if (d.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(d);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, senseRadius);
    }

    // giảm velocity khi tiếp cận nạn nhân
     void UpdateApproachSpeed(Vector3 ipos)
    {
        // mặc định chạy ở tốc độ gốc
        float desired = baseSpeed;

        // khoảng cách còn lại tới điểm tương tác
        float dist = Vector3.Distance(transform.position, ipos);

        // nếu đã vào vùng cần giảm tốc: nội suy về minApproachSpeed
        if (dist < slowDownStartDist)
        {
            // t=1 ở mép ngoài, t=0 sát điểm dừng (tính cả stoppingDistance)
            float t = Mathf.InverseLerp(Mathf.Max(agent.stoppingDistance, 0.01f),
                                        slowDownStartDist, dist);
            desired = Mathf.Lerp(minApproachSpeed, baseSpeed, t);
        }

        // (tuỳ chọn) spherecast phía trước để chỉ giảm tốc khi thật sự thấy nạn nhân
        if (useRaySlowdown && target != null)
        {
            Vector3 eye = transform.position + Vector3.up * (eyeHeight * 0.5f);
            Vector3 to  = (target.GetInteractPos() - eye);
            float   d   = to.magnitude;
            if (d > 0.01f)
            {
                Vector3 dir = to / d;
                if (Physics.SphereCast(eye, slowRayRadius, dir,
                                       out RaycastHit hit,
                                       Mathf.Min(d, slowDownStartDist),
                                       victimMask,
                                       QueryTriggerInteraction.Ignore))
                {
                    // khi ray chạm nạn nhân trong vùng chậm, “siết” tốc hơn một chút
                    desired = Mathf.Min(desired,
                              Mathf.Lerp(minApproachSpeed, baseSpeed, d / slowDownStartDist));
                }
            }
        }

        // mượt mà đưa speed hiện tại về desired
        agent.speed = Mathf.MoveTowards(agent.speed, desired, speedLerp * Time.deltaTime);
    }
}
