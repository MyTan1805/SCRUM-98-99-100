
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Animator animator;
    int isWalkingHash;
    int isRunningHash;
    private int speedHash;

    [Header("Speed")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;
    public float turnSpeed = 180f;
    // [SerializeField] private float rotationSpeed = 720f;
    private float currentSpeed = 0f;

    private Vector2 moveInput; // WASD
    private bool runHeld; // Shift/stick press

    int velocityHash;


    void Start()
    {

    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        speedHash = Animator.StringToHash("Velocity");
    }
    // Update is called once per frame
    void Update()
    {
        Move();
    }
    
    void Move()
    {
        // --- Input legacy ---
        float v = Input.GetAxisRaw("Vertical");   // W/S (-1..1)
        float h = Input.GetAxisRaw("Horizontal"); // A/D (-1..1)
        bool runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // --- Quay thân (A/D) ---
        transform.Rotate(0f, h * turnSpeed * Time.deltaTime, 0f);

        // --- Tăng/giảm tốc mượt ---
        float target = Mathf.Abs(v) * (runHeld ? runSpeed : walkSpeed);
        float accel = target > currentSpeed ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, target, accel * Time.deltaTime);

        // --- Tiến/lùi theo mặt nhân vật ---
        float signedSpeed = Mathf.Sign(v) * currentSpeed;
        transform.position += transform.forward * signedSpeed * Time.deltaTime;

        // --- Animator (tùy chọn) ---
        bool moving  = Mathf.Abs(v) > 0.01f && currentSpeed > 0.01f;
        bool running = moving && runHeld;
        animator.SetBool(isWalkingHash, moving && !running);
        animator.SetBool(isRunningHash, running);
        animator.SetFloat(speedHash, currentSpeed);
    }
}
