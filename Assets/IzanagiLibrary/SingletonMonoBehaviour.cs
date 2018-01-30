using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IzanagiLibrary.Utility
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                System.Type type = typeof(T);
                T instance = FindObjectOfType(type) as T;
                if (instance != null)
                {
                    _instance = instance;
                    return _instance;
                }

                var name = type.ToString();
                var obj = new GameObject(name, type);
                instance = obj.GetComponent<T>();
                if (instance != null)
                {
                    _instance = instance;
                    return instance;
                }

                return null;
            }
        }

        /// <summary>
        /// インスタンスが生成済み or 破棄されていないかのチェック
        /// </summary>
        public static bool IsValid
        {
            get { return _instance != null; }
        }

        private void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // 入れ子になっているとDontDestroyに設定できないため、親をnullにする
            gameObject.transform.parent = null;

            DontDestroyOnLoad(gameObject);

            OnAwake();
        }

        protected virtual void OnAwake() { }
    }
}