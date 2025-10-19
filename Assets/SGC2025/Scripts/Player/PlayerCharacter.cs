using UnityEngine;


//namespace SGC2025


    public class PlayerCharacter : MonoBehaviour
    {

        public Animator anim {  get; private set; }
        public Rigidbody rb { get; private set; }

        public PlayerInputSet input;
        public StateMachine stateMachine { get; private set; }




        public PlayerIdleState idleState {  get; private set; }
        public PlayerMoveState moveState {  get; private set; }






        [Header("ステータス")]
        //体力
        [SerializeField] private float maxHealth = 30;
        private float currentHealth;
        //移動速度
        public float moveSpeed;
        //無敵時間
        [SerializeField] private float mutekiTime;
        private float nowMutekiTime;

        //[Header("移動速度")]
        public Vector2 moveInput {  get; private set; }


        [Space]
        [Header("移動制限")]
        [SerializeField] public Vector2 positionLimitHigh;
        [SerializeField] public Vector2 positionLimitLow;





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

            currentHealth = maxHealth;
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
            PlayerRotate();
        }

        private void PlayerRotate()
        {
            //プレイヤーの回転
            if(moveInput != Vector2.zero)
                transform.up = rb.linearVelocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            //ダメージ判定
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                Damage(10);
            }
        }


        public void SetVelocity(float moveInputX, float moveInputY)
        {
            Vector2 moveInputNormalized = new Vector2(moveInputX, moveInputY).normalized;
            rb.linearVelocity = new Vector2(moveInputNormalized.x * moveSpeed, moveInputNormalized.y * moveSpeed);

        }


        //private void PositionLimit()
        //{

        //}

        private void DecreaseMutekiTime()
        {
            nowMutekiTime -= Time.deltaTime;
        }


        private void Damage(int value)
        {
        
            if (nowMutekiTime > 0f)
                return;

            //ここにダメージ処理を追加
            Debug.Log("Player damaged");

            currentHealth -= value;

            nowMutekiTime = mutekiTime;
        }

        //プレイヤーの有効化
        private void PlayerActive()
        {
            gameObject.SetActive(true);
        }

        //プレイヤーの非有効化
        private void PlayerInactive()
        {
            gameObject.SetActive(false);
        }

        public float GetPlayerMaxHealth()
        {

            return maxHealth;
        }
        public float GetPlayerCurrentHalth()
        {

            return currentHealth;
        }

    }

