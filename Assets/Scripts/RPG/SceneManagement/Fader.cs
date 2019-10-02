using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        private CanvasGroup m_canvasGroup;

        private void Start()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Instantly fade out
        /// </summary>
        public void FadeOutImmediate()
        {
            m_canvasGroup.alpha = 1;
        }

        /// <summary>
        /// Fade out of the scene
        /// </summary>
        /// <param name="duration">Duration of the fade</param>
        /// <returns></returns>
        public IEnumerator FadeOut(float duration)
        {
            while (m_canvasGroup.alpha < 1)
            {
                m_canvasGroup.alpha += Time.deltaTime / duration;
                yield return null;
            }
        }

        /// <summary>
        /// Fade into the scene
        /// </summary>
        /// <param name="duration">Duration of the fade</param>
        /// <returns></returns>
        public IEnumerator FadeIn(float duration)
        {
            while (m_canvasGroup.alpha > 0)
            {
                m_canvasGroup.alpha -= Time.deltaTime / duration;
                yield return null;
            }
        }
    }
}
