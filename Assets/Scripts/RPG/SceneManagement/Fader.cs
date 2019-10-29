using System.Collections;
using UnityEngine;

namespace RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        private CanvasGroup m_canvasGroup;
        private Coroutine m_currentlyActiveFade;

        private void Awake()
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
        public Coroutine FadeOut(float duration)
        {
            return Fade(1, duration);
        }

        /// <summary>
        /// Fade into the scene
        /// </summary>
        /// <param name="duration">Duration of the fade</param>
        /// <returns></returns>
        public Coroutine FadeIn(float duration)
        {
            return Fade(0, duration);
        }

        public Coroutine Fade(float alphaTarget, float duration)
        {
            if (m_currentlyActiveFade != null)
            {
                StopCoroutine(m_currentlyActiveFade);
            }
            m_currentlyActiveFade = StartCoroutine(FadeRoutine(alphaTarget, duration));
            return m_currentlyActiveFade;
        }

        private IEnumerator FadeRoutine(float alphaTarget, float duration)
        {
            while (!Mathf.Approximately(m_canvasGroup.alpha, alphaTarget))
            {
                m_canvasGroup.alpha = Mathf.MoveTowards(m_canvasGroup.alpha, alphaTarget, Time.deltaTime / duration);
                yield return null;
            }
        }
    }
}
