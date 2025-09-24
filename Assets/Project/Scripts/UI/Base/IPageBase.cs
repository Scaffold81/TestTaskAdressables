namespace Game.UI
{
    /// <summary>
    /// Базовый интерфейс для всех UI страниц. Определяет основные операции показа и скрытия.
    /// Base interface for all UI pages. Defines basic show and hide operations.
    /// </summary>
    public interface IPageBase
    {
        /// <summary>
        /// Скрывает страницу с анимацией.
        /// Hides page with animation.
        /// </summary>
        /// <param name="hideTime">Время анимации скрытия / Hide animation time</param>
        void Hide(float hideTime = 0.1F);
        
        /// <summary>
        /// Показывает страницу с анимацией.
        /// Shows page with animation.
        /// </summary>
        /// <param name="showTime">Время анимации появления / Show animation time</param>
        void Show(float showTime = 0.1F);
        
        /// <summary>
        /// Показывает страницу поверх всех остальных с анимацией.
        /// Shows page on top of all others with animation.
        /// </summary>
        /// <param name="showTime">Время анимации появления / Show animation time</param>
        void ShowAsLastSibling(float showTime = 0.1F);
    }
}
