using DG.Tweening;
using R3;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Базовый класс для всех UI страниц. Предоставляет базовую функциональность для анимаций появления/скрытия.
    /// Base class for all UI pages. Provides basic functionality for show/hide animations.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PageBase : MonoBehaviour, IPageBase
    {
        [SerializeField]
        protected CanvasGroup canvasGroup;
        protected float durationTime = 0.1f;
       
        protected ReactiveProperty<bool> isShowed = new ReactiveProperty<bool>();
        
        /// <summary>
        /// Показывает страницу поверх всех остальных с анимацией появления.
        /// Shows page on top of all others with show animation.
        /// </summary>
        /// <param name="showTime">Время анимации появления / Show animation time</param>
        public virtual void ShowAsLastSibling(float showTime = 0.1f)
        {
            transform.SetAsLastSibling();
            
            canvasGroup.DOFade(1, showTime).OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
        }

        /// <summary>
        /// Показывает страницу с анимацией появления.
        /// Shows page with show animation.
        /// </summary>
        /// <param name="showTime">Время анимации появления / Show animation time</param>
        public virtual void Show(float showTime = 0.1f)
        {
            isShowed.Value = true;
            canvasGroup.DOFade(1, showTime).OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
        }

        /// <summary>
        /// Скрывает страницу с анимацией скрытия.
        /// Hides page with hide animation.
        /// </summary>
        /// <param name="hideTime">Время анимации скрытия / Hide animation time</param>
        public virtual void Hide(float hideTime = 0.1f)
        {
            isShowed.Value = false;
            canvasGroup.DOFade(0, hideTime).OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            });
        }
    }
}
