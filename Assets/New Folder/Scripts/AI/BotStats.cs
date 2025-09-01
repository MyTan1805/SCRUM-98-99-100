using UnityEngine;

[CreateAssetMenu(fileName = "NewBotStats", menuName = "Firefighter/Bot Stats", order = 0)]
public class BotStats : ScriptableObject
{
    [Header("Core Stats")]
    [Tooltip("Lượng máu tối đa.")]
    public float maxHealth = 100f;

    [Tooltip("Lượng oxy tối đa trong bình.")]
    public float maxOxygen = 200f; 

    [Header("Movement")]
    [Tooltip("Tốc độ di chuyển của NavMesh Agent.")]
    public float moveSpeed = 3.5f;

    [Tooltip("Tốc độ xoay của NavMesh Agent.")]
    public float angularSpeed = 120f;

    [Header("Interaction Power")]
    [Tooltip("Sức mạnh dập lửa mỗi giây.")]
    public float extinguishPower = 10f;

    [Tooltip("Sát thương gây ra cho cửa mỗi lần tấn công.")]
    public float breakDoorDamage = 25f;

    [Header("Behavioral Thresholds")]
    [Tooltip("Ngưỡng oxy còn lại để bot tự động rút lui (tính theo giá trị tuyệt đối, không phải %).")]
    public float lowOxygenThreshold = 50f;

    [Tooltip("Khoảng cách tối đa để có thể bắt đầu tương tác (dập lửa, phá cửa).")]
    public float interactionRange = 2.0f;
}