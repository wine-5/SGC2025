using UnityEngine;

public class Player : MonoBehaviour
{

    public Animator anim {  get; private set; }
    public Rigidbody rb { get; private set; }

    public PlayerInputSet input;
    public StateMachine stateMachine { get; private set; }




    public PlayerIdleState idleState {  get; private set; }
    public PlayerMoveState moveState {  get; private set; }







    [Header("�ړ����x")]
    public float moveSpeed;
    public Vector2 moveInput {  get; private set; }


    [Header("�X�e�[�^�X")]
    [SerializeField] private int health = 30;



    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        stateMachine = new StateMachine();
        input = new PlayerInputSet();

        //�X�e�[�g�� = new �N���X��(this, stateMachine, "animator�Őݒ肵��bool��")
        idleState = new PlayerIdleState(this, stateMachine, "fly");
        moveState = new PlayerMoveState(this, stateMachine, "fly");
    }


    private void OnEnable()
    {
        input.Enable();

        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.UpdateActiveState();

        
    }


    public void SetVelocity(float moveInputX, float moveInputY)
    {
        Vector2 moveInputNormalized = new Vector2(moveInputX, moveInputY).normalized;
        rb.linearVelocity = new Vector2(moveInputNormalized.x * moveSpeed, moveInputNormalized.y * moveSpeed);

    }

}
