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




        public void ResetValue()
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

            currentCountDownTimer = startCountDownTime;
            isCountDown = true;
        }

        public float CountDownTimer()
        {
            //カウントダウンの実行 Update想定
            if (!isCountDown)
                return 0f;
            currentCountDownTimer -= Time.deltaTime;

            if (currentCountDownTimer <= 0f)
                isCountDown = false;
            return currentCountDownTimer;
        }

        public float CountGameTimer()
        {
            //ゲーム時間のカウント実行 Update想定
            if (isCountDown)
                return 0f;

            countGameTimer += Time.deltaTime;

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
