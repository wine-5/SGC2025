using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SGC2025
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        [Header("スコア設定")]
        [SerializeField]
        [Tooltip("通常タイルを緑化した際のポイント")]
        private int normalTilePoint = 100;
        
        [SerializeField]
        [Tooltip("ハイスコアタイルのポイント倍率")]
        private int highScoreTileMultiplier = 3;
        
        /// <summary>通常タイルポイントを取得</summary>
        public int NormalTilePoint => normalTilePoint;
        
        /// <summary>ハイスコア倍率を取得</summary>
        public int HighScoreTileMultiplier => highScoreTileMultiplier;
        
        [Header("タイマー設定")]
        //スタート時のカウントダウン時間
        [SerializeField] private float startCountDownTime;
        [SerializeField] private InGameUI gameScoreUI;
        //時間経過の計測 カウントダウン&ゲーム時間
        private bool isCountDown = false;
        private float currentCountDownTimer = 0f;
        private float countGameTimer = 0f;

        //エネミースコア
        private int scoreEnemy = 0;

        //塗り絵スコア
        private int scoreGreen = 0;



        private void Start()
        {
            ResetValue();

            //CountDownStart();
        }

        private void Update()
        {
            CountDownTimer();
            CountGameTimer();

            if(CommonDef.GAME_MINIT <= countGameTimer)
            { // おわり
                SceneManager.LoadScene("Result");
            }
            
            //Debug.Log("countDown:" + GetCountDown() + ", gameCount:" + GetGameCount());
        }


        private void ResetValue()
        {
            //�J�n���Ɏ��s �l�̃��Z�b�g
            isCountDown = false;
            currentCountDownTimer = startCountDownTime;
            countGameTimer = 0f;
            scoreEnemy = 0;
            scoreGreen = 0;
        }


        public void CountDownStart()
        {
            //isCountDown��true�̏ꍇ�A�J�E���g�_�E�������s
            //�Q�[���X�^�[�g���ɌĂяo��

            currentCountDownTimer = startCountDownTime;
            isCountDown = true;
        }

        private void CountDownTimer()
        {
            //�J�E���g�_�E���̎��s Update�z��
            if (!isCountDown)
                return ;
            currentCountDownTimer -= Time.deltaTime;

            if (currentCountDownTimer <= 0f)
                isCountDown = false;
            //return currentCountDownTimer;
        }

        public float GetCountDown()
        {
            return currentCountDownTimer;
        }

        private void CountGameTimer()
        {
            //�Q�[�����Ԃ̃J�E���g���s Update�z��
            if (currentCountDownTimer > 0f || isCountDown)
                return ;

            countGameTimer += Time.deltaTime;

            //return countGameTimer;
        }

        public float GetGameCount()
        {
            return CommonDef.GAME_MINIT - countGameTimer;
        }


        public void AddEnemyScore(int score)
        {
            //�G�l�~�[�X�R�A�̉��Z
            scoreEnemy += score;
            CommonDef.currentEnemyScore = score;
            gameScoreUI.ShowScorePopup(score);
        }

        public int GetEnemyScore()
        {
            //�G�l�~�[�X�R�A�̎擾
            return scoreEnemy;
        }

        public void AddGreenScore(int score)
        {
            //�Ή��X�R�A�̉��Z
            scoreGreen += score;
            CommonDef.currentGreeningScore = scoreGreen;
            gameScoreUI.ShowScorePopup(score);
        }

        public int GetGreenScore()
        {
            //�Ή��X�R�A�̎擾
            return scoreGreen;
        }

    }
}
