using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IzanagiLibrary
{
    public class ShaderPropertyChanger : MonoBehaviour
    {
        [SerializeField]
        private Renderer _renderer;
        [SerializeField]
        private string _propertyName;
        [SerializeField]
        private float _startValue;
        [SerializeField]
        private float _goalValue;
        [SerializeField]
        private float _duration;

        private bool _startChange = false;
        private MaterialPropertyBlock _materialPropertyBlock;

        public Action OnStart;
        public Action OnFinish;

        [ContextMenu("ChangeProperty")]
        public void ChangeProperty()
        {
            if(_startChange)
            {
                return;
            }

            StartCoroutine(ChangePropertyImpl(_startValue, _goalValue, _duration));
            _startChange = true;
        }

        public void ChangeProperty(float start, float goal, float duration)
        {
            if (_startChange)
            {
                return;
            }

            StartCoroutine(ChangePropertyImpl(start, goal, duration));
            _startChange = true;
        }

        private IEnumerator ChangePropertyImpl(float start, float goal, float duration)
        {
            if(OnStart != null)
            {
                OnStart.Invoke();
                OnStart = null;
            }

            _materialPropertyBlock = new MaterialPropertyBlock();
            _materialPropertyBlock.Clear();

            _materialPropertyBlock.SetFloat(_propertyName, start);
            _renderer.SetPropertyBlock(_materialPropertyBlock);

            float time = 0;
            while (true)
            {
                time += Time.deltaTime;
                if (time >= duration)
                {
                    break;
                }

                var t = time / duration;

                _materialPropertyBlock.SetFloat(_propertyName, Mathf.Lerp(start, goal, t));
                _renderer.SetPropertyBlock(_materialPropertyBlock);
                yield return null;
            }

            _materialPropertyBlock.SetFloat(_propertyName, goal);
            _renderer.SetPropertyBlock(_materialPropertyBlock);

            if (OnFinish != null)
            {
                OnFinish.Invoke();
                OnFinish = null;
            }

            _startChange = false;
        }
    }
}