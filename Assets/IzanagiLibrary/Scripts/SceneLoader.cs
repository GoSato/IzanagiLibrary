using IzanagiLibrary.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IzanagiLibrary
{
    /// <summary>
    /// シーンをロードする
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField]
        private string[] _sceneList;

        private void Start()
        {
            SceneController.Instance.LoadAdditiveScene(_sceneList);
        }
    }
}