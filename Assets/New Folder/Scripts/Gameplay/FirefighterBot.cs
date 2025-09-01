using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FirefighterBot : MonoBehaviour, IAgent
{
    [Header("Configuration")]
    [Tooltip("Gán asset BotStats chứa các chỉ số cho bot này.")]
    public BotStats stats;

    [Header("Runtime State")]
    [SerializeField] private ITargetable currentTarget; 
    private float currentHealth;
    private float currentOxygen;

    // Components
    private NavMeshAgent navMeshAgent;

    #region Unity Lifecycle

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        ApplyStats();
    }
    
    private void Update()
    {
        // Ví dụ logic tiêu thụ oxy
        // Logic thực tế có thể phức tạp hơn, ví dụ: chỉ tiêu thụ khi ở trong vùng khói
        // ConsumeOxygen(1f * Time.deltaTime);

        // Cập nhật các biến trên Blackboard liên tục (nếu cần)
        // behaviorTreeRunner.Blackboard.SetValue("OxygenLevel", currentOxygen);
    }

    #endregion

    #region API & Core Logic

    private void ApplyStats()
    {
        if (stats != null)
        {
            currentHealth = stats.maxHealth;
            currentOxygen = stats.maxOxygen;
            navMeshAgent.speed = stats.moveSpeed;
            navMeshAgent.angularSpeed = stats.angularSpeed;
            navMeshAgent.stoppingDistance = stats.interactionRange * 0.9f; 
        }
        else
        {
            Debug.LogError("BotStats not assigned on " + gameObject.name + "! AI will not function correctly.");
        }
    }

    public void SetTarget(ITargetable target)
    {
        Debug.Log(gameObject.name + " received a new target: " + (target as MonoBehaviour)?.gameObject.name);
        currentTarget = target;
        
        // Ví dụ cho Unity Behavior:
        // var blackboard = GetComponent<BehaviorGraphAgent>().BlackboardReference;
        // blackboard.SetVariable("CommanderTarget", new BlackboardVariable<ITargetable>(target));
    }

    public void ConsumeOxygen(float amount)
    {
        if (currentOxygen > 0)
        {
            currentOxygen -= amount;
            if (currentOxygen <= 0)
            {
                currentOxygen = 0;
                Debug.LogWarning(gameObject.name + " ran out of oxygen!");
            }
        }
    }

    #endregion
}