using UnityEngine;

namespace SGC2025
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        //�X�^�[�g���̃J�E���g�_�E������
        [SerializeField] private float startCountDownTime;

        //���Ԍo�߂̌v���@��������&�o�ߎ���
        private bool isCountDown = false;
        private float currentCountDownTimer = 0f;
        private float countGameTimer = 0f;

        //�G�l�~�[�X�R�A
        private int scoreEnemy = 0;

        //�Ή��X�R�A
        private int scoreGreen = 0;




        public void ResetValue()
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

            currentCountDownTimer = startCountDownTime;
            isCountDown = true;
        }

        public float CountDownTimer()
        {
            //�J�E���g�_�E���̎��s Update�z��
            if (!isCountDown)
                return 0f;
            currentCountDownTimer -= Time.deltaTime;

            if (currentCountDownTimer <= 0f)
                isCountDown = false;
            return currentCountDownTimer;
        }

        public float CountGameTimer()
        {
            //�Q�[�����Ԃ̃J�E���g���s Update�z��
            if (isCountDown)
                return 0f;

            countGameTimer += Time.deltaTime;

            return countGameTimer;
        }


        public void AddEnemyScore(int score)
        {
            //�G�l�~�[�X�R�A�̉��Z

            scoreEnemy += score;
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
        }

        public int GetGreenScore()
        {
            //�Ή��X�R�A�̎擾
            return scoreGreen;
        }

    }
}
