using UnityEngine;

namespace SGC2025
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        //スタート時のカウントダウン時間
        [SerializeField] private float startCountDownTime;

        //時間経過の計測　制限時間&経過時間
        private bool isCountDown = false;
        private float currentCountDownTimer = 0f;
        private float countGameTimer = 0f;

        //エネミースコア
        private int scoreEnemy = 0;

        //緑化スコア
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




            
            //Debug.Log("countDown:" + GetCountDown() + ", gameCount:" + GetGameCount());
        }


        private void ResetValue()
        {
            //開始時に実行 値のリセット
            isCountDown = false;
            currentCountDownTimer = startCountDownTime;
            countGameTimer = 0f;
            scoreEnemy = 0;
            scoreGreen = 0;
        }


        public void CountDownStart()
        {
            //isCountDownがtrueの場合、カウントダウンが実行
            //ゲームスタート時に呼び出す

            currentCountDownTimer = startCountDownTime;
            isCountDown = true;
        }

        private void CountDownTimer()
        {
            //カウントダウンの実行 Update想定
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
            //ゲーム時間のカウント実行 Update想定
            if (currentCountDownTimer > 0f || isCountDown)
                return ;

            countGameTimer += Time.deltaTime;

            //return countGameTimer;
        }

        public float GetGameCount()
        {
            return countGameTimer;
        }


        public void AddEnemyScore(int score)
        {
            //エネミースコアの加算

            scoreEnemy += score;
        }

        public int GetEnemyScore()
        {
            //エネミースコアの取得
            return scoreEnemy;
        }

        public void AddGreenScore(int score)
        {
            //緑化スコアの加算

            scoreGreen += score;
        }

        public int GetGreenScore()
        {
            //緑化スコアの取得
            return scoreGreen;
        }

    }
}
