using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Animations;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

namespace SGC2025
{
    public class UIBase : MonoBehaviour
    {
        [SerializeField]
        protected GameObject firstSelect;
        protected float waitTime = 0.0f;
        protected UIBase parentUI;

        protected List<UIBase> childrenMenu;

        virtual public void Start()
        {
            childrenMenu = new List<UIBase>();
            if (firstSelect != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelect);
            }
        }
        
        virtual public void Update()
        {
            waitTime += Time.unscaledDeltaTime;
        }
        public void OnClickRestart()
        {
            SceneManager.LoadScene("InGame");
        }

        public void OnClickBackTitle()
        {
            SceneManager.LoadScene("Title");
        }
        
        public void OnClickControllGuide()
        {
            //操作説明用ガイドプレハブ作成
        }

        public void OnClickExit()
        {
            Application.Quit();
        }

        protected int ScoreCountUp(float currentWaitTime, float scoreMaxValue, float waitMaxTime)
        {
            float t = Mathf.Clamp01(currentWaitTime / waitMaxTime);
            return (int)Mathf.Lerp(0f, scoreMaxValue, t);
        }

        protected UIBase CreateMenu( string path )
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            GameObject childMenu = Instantiate(prefab);

            childMenu.gameObject.name = childMenu.gameObject.name.Replace("(Clone)", "");
            UIBase childUIBase = childMenu.GetComponent<UIBase>();

            if(childUIBase == null){ return null; }
            childUIBase.parentUI = this;
            childrenMenu.Add(childUIBase);

            return childUIBase;
        }

        virtual protected void DestoryChild(UIBase uIBase)
        {
            UIBase destoryChild = childrenMenu.Find(x => x == uIBase);
            // デリゲートでやったらすごく良い
        }

        public void OnDestroy()
        {
            if (parentUI != null)
            {
                // 子が死んだら親に死んだことを通知
                parentUI.DestoryChild(this);
            }

            if( childrenMenu.Count > 0 )
            {
                foreach(UIBase child in childrenMenu)
                {
                    if(child==null ||child.gameObject ==null)
                    {
                        //多分ロード破棄
                        continue;
                    }
                    // 親が死んだら子も死ぬ
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
