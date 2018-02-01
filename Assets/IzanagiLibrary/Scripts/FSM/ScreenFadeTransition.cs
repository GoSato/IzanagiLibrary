using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IzanagiLibrary.FSM
{
    public class ScreenFadeTransition : IStateTransition
    {
        private Canvas canvas;
        private Image cover;
        private float fadeTime;

        public ScreenFadeTransition(float fadeTime)
        {
            var screenFadePrefab = Resources.Load<Canvas>("ScreenFade");
            canvas = UnityEngine.Object.Instantiate(screenFadePrefab);
            var coverGO = canvas.transform.Find("Cover");
            cover = coverGO.GetComponent<Image>();
            this.fadeTime = fadeTime;
        }

        public IEnumerable Exit()
        {
            foreach(var e in TweenAlpha(0, 1, fadeTime / 2))
            {
                yield return e;
            }
        }

        public IEnumerable Enter()
        {
            foreach(var e in TweenAlpha(1, 0, fadeTime / 2))
            {
                yield return e;
            }

            UnityEngine.Object.Destroy(canvas.gameObject);
        }

        private IEnumerable TweenAlpha(float fromAlpha, float toAlpha, float duration)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;
            while(Time.time < endTime)
            {
                var sinceStart = Time.time - startTime; // 経過時間
                var percent = sinceStart / duration;
                var color = cover.color;
                color.a = Mathf.Lerp(fromAlpha, toAlpha, percent);
                cover.color = color;
                yield return null;
            }
        }

    }
}