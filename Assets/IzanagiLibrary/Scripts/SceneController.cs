using IzanagiLibrary.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IzanagiLibrary.Management
{
    /// <summary>
    /// シーン読み込みの制御を行う
    /// </summary>
    public class SceneController : SingletonMonoBehaviour<SceneController>
    {
        public void LoadAdditiveScene(string[] additiveSceneList)
        {
            StartCoroutine(LoadAdditiveSceneImpl(additiveSceneList));
        }

        private IEnumerator LoadAdditiveSceneImpl(string[] additiveSceneList)
        {
            AsyncOperation[] asyncLoadList = new AsyncOperation[additiveSceneList.Length];

            for (int i = 0; i < additiveSceneList.Length; i++)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(additiveSceneList[i], LoadSceneMode.Additive);
                asyncLoadList[i] = asyncLoad;
                asyncLoad.allowSceneActivation = false;

                while(true)
                {
                    float progress = asyncLoad.progress;

                    if(progress >= 0.9f)
                    {
                        break;
                    }

                    yield return null;
                }
            }

            for(int i = 0; i < asyncLoadList.Length; i++)
            {
                asyncLoadList[i].allowSceneActivation = true;
            }
        }
    }
}