using UnityEngine;
using SGC2025;
using System;

/// <summary>
/// プレイヤーキャラクターの制御クラス
/// </summary>
public class Player : Singleton<Player>
{
    #region 静的プロパティ
    /// <summary>プレイヤーのTransform（静的参照用）</summary>
    public static Transform PlayerTransform => I?.transform;
    /// <summary>プレイヤーの現在位置（静的参照用）</summary>
    public static Vector3 PlayerPosition => I?.transform.position ?? Vector3.zero;
    /// <summary>DontDestroyOnLoadを使用しない（シーンごとに再生成）</summary>
    protected override bool UseDontDestroyOnLoad => false;
    #endregion

    #region コンポーネント
    public Animator anim { get; private set; }
    public Rigidbody rb { get; private set; }
    public PlayerInputSet input { get; private set; }
    public StateMachine stateMachine { get; private set; }
    #endregion

    #region ステート
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    #endregion

    #region 体力システム
    [Header("ステータス")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;
    public float HealthPercentage => maxHealth > 0 ? (currentHealth / maxHealth) * 100f : 0f;
    public bool IsInvincible => nowMutekiTime > 0f;
    #endregion

    #region イベント
    /// <summary>ダメージを受けた時のイベント</summary>
    public static event Action<float> OnPlayerDamaged;
    #endregion

    #region 移動システム
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public Vector2 moveInput { get; private set; }
    [Header("移動制限")]
    [SerializeField] public Vector2 positionLimitHigh;
    [SerializeField] public Vector2 positionLimitLow;
    #endregion

    #region ダメージシステム
    [Header("ダメージ設定")]
    [SerializeField] private float mutekiTime = 1f;
    private float nowMutekiTime;
    #endregion

    #region Unityライフサイクル
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void Init()
    {
        // コンポーネント取得
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        // システム初期化
        stateMachine = new StateMachine();
        input = new PlayerInputSet();
        // ステート作成
        idleState = new PlayerIdleState(this, stateMachine, "fly");
        moveState = new PlayerMoveState(this, stateMachine, "fly");
        // 体力初期化
        currentHealth = maxHealth;
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

    /// <summary>
    /// 敵との接触判定（OnTriggerEnter）
    /// 敵に当たったら体力を減らす
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 無敵時間中はダメージを受けない
        if (IsInvincible)
        {
            return;
        }

        // 敵レイヤーとの接触チェック
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            HandleEnemyContact(other.gameObject);
        }
    }

    /// <summary>
    /// 敵との接触処理
    /// </summary>
    /// <param name="enemy">接触した敵オブジェクト</param>
    private void HandleEnemyContact(GameObject enemy)
    {
        // EnemyControllerからダメージ値を取得
        var enemyController = enemy.GetComponent<SGC2025.Enemy.EnemyController>();
        
        if (enemyController != null)
        {
            float damage = enemyController.AttackPower;
            Debug.Log($"敵 {enemy.name} と接触 - ダメージ: {damage}");
            TakeDamage(damage);
        }
        else
        {
            // EnemyControllerが見つからない場合のフォールバック
            Debug.LogWarning($"敵 {enemy.name} にEnemyControllerが見つかりません - デフォルトダメージを適用");
            TakeDamage(10f);
        }
    }
    #endregion

    #region 移動メソッド
    /// <summary>
    /// プレイヤーの移動速度を設定
    /// </summary>
    /// <param name="moveInputX">X軸入力</param>
    /// <param name="moveInputY">Y軸入力</param>
    public void SetVelocity(float moveInputX, float moveInputY)
    {
        Vector2 moveInputNormalized = new Vector2(moveInputX, moveInputY).normalized;
        rb.linearVelocity = new Vector2(moveInputNormalized.x * moveSpeed, moveInputNormalized.y * moveSpeed);
    }
    #endregion

    #region プライベートメソッド
    private void DecreaseMutekiTime()
    {
        nowMutekiTime -= Time.deltaTime;
    }
    #endregion

    #region 体力システムメソッド
    /// <summary>
    /// ダメージを受ける処理（敵のAttackPowerを使用）
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public void TakeDamage(float damage)
    {
        // ダメージ値の検証
        if (damage <= 0)
        {
            Debug.LogWarning($"無効なダメージ値: {damage}");
            return;
        }
        
        // 体力を減らす
        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        float actualDamage = previousHealth - currentHealth;
        
        Debug.Log($"プレイヤーダメージ: -{actualDamage} HP (残り体力: {currentHealth}/{maxHealth})");
        
        // 無敵時間を設定
        nowMutekiTime = mutekiTime;
        
        // ダメージイベントを発火
        OnPlayerDamaged?.Invoke(actualDamage);
        
        // 体力が0になったら死亡処理
        if (!IsAlive)
        {
            OnPlayerDeath();
        }
    }
    
    /// <summary>
    /// 体力回復
    /// </summary>
    /// <param name="healAmount">回復量</param>
    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        Debug.Log($"Player healed: +{healAmount} HP, Current HP: {currentHealth}/{maxHealth}");
    }
    
    /// <summary>
    /// 体力を最大値まで回復
    /// </summary>
    public void FullHeal()
    {
        currentHealth = maxHealth;
        Debug.Log($"Player fully healed: HP: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// プレイヤー死亡時の処理
    /// </summary>
    private void OnPlayerDeath()
    {
        Debug.Log("Player Death!");
        // TODO: ゲームオーバー画面表示、リスポーン処理など
    }
    #endregion
}
