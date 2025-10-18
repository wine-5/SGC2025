using UnityEngine;

public class Player : MonoBehaviour
{

    public Animator anim {  get; private set; }
    public Rigidbody rb { get; private set; }

    public PlayerInputSet input;
    public StateMachine stateMachine { get; private set; }




    public PlayerIdleState idleState {  get; private set; }
    public PlayerMoveState moveState {  get; private set; }






    [Header("ステータス")]
    //[SerializeField] private int health = 30;

    public float moveSpeed;
    [SerializeField] private float mutekiTime;
    private float nowMutekiTime;

    //[Header("移動速度")]
    public Vector2 moveInput {  get; private set; }





    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        stateMachine = new StateMachine();
        input = new PlayerInputSet();

        //ステート名 = new クラス名(this, stateMachine, "animatorで設定したbool名")
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

        DecreaseMutekiTime();
    }


    private void OnTriggerEnter(Collider other)
    {
        //ダメージ判定
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Damage();
        }
    }


    public void SetVelocity(float moveInputX, float moveInputY)
    {
        Vector2 moveInputNormalized = new Vector2(moveInputX, moveInputY).normalized;
        rb.linearVelocity = new Vector2(moveInputNormalized.x * moveSpeed, moveInputNormalized.y * moveSpeed);

    }


    public void Damage()
    {
        //ダメージの処理

        if (nowMutekiTime > 0f)
            return;

        Debug.Log("Player damaged");

        nowMutekiTime = mutekiTime;
    }

    private void DecreaseMutekiTime()
    {
        nowMutekiTime -= Time.deltaTime;
    }

}
