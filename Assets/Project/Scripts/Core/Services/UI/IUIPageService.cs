using Game.UI;
using System;
using System.Collections.Generic;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс сервиса управления UI страницами.
    /// UI page management service interface.
    /// </summary>
    public interface IUIPageService
    {
        /// <summary>
        /// Словарь зарегистрированных UI страниц.
        /// Dictionary of registered UI pages.
        /// </summary>
        Dictionary<Type, IPageBase> Pages { get;}

        /// <summary>
        /// Добавляет страницу в словарь для дальнейшего управления.
        /// Adds page to dictionary for further management.
        /// </summary>
        /// <param name="key">Ключ типа страницы / Page type key</param>
        /// <param name="page">Объект страницы / Page object</param>
        void AddPage(Type key, IPageBase page);
        
        /// <summary>
        /// Показывает страницу.
        /// Shows page.
        /// </summary>
        /// <param name="key">Ключ типа страницы / Page type key</param>
        void ShowOn(Type key);
        
        /// <summary>
        /// Показывает страницу поверх всех остальных.
        /// Shows page on top of all others.
        /// </summary>
        /// <param name="key">Ключ типа страницы / Page type key</param>
        void ShowOnTop(Type key);
    }
}
