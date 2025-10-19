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






        [Header("�X�e�[�^�X")]
        //�̗�
        [SerializeField] private float maxHealth = 30;
        private float currentHealth;
        //�ړ����x
        public float moveSpeed;
        //���G����
        [SerializeField] private float mutekiTime;
        private float nowMutekiTime;

        //[Header("�ړ����x")]
        public Vector2 moveInput {  get; private set; }


        [Space]
        [Header("�ړ�����")]
        [SerializeField] public Vector2 positionLimitHigh;
        [SerializeField] public Vector2 positionLimitLow;





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
            //�v���C���[�̉�]
            if(moveInput != Vector2.zero)
                transform.up = rb.linearVelocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            //�_���[�W����
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

            //�����Ƀ_���[�W������ǉ�
            Debug.Log("Player damaged");

            currentHealth -= value;

            nowMutekiTime = mutekiTime;
        }

        //�v���C���[�̗L����
        private void PlayerActive()
        {
            gameObject.SetActive(true);
        }

        //�v���C���[�̔�L����
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

