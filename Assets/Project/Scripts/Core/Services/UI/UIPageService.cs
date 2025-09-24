using Game.UI;
using System;
using System.Collections.Generic;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Сервис для управления UI страницами. Позволяет открывать и закрывать окна.
    /// UI page management service. Allows opening and closing windows.
    /// </summary>
    public class UIPageService : IUIPageService
    {
        /// <summary>
        /// Словарь зарегистрированных UI страниц.
        /// Dictionary of registered UI pages.
        /// </summary>
        public Dictionary<Type, IPageBase> Pages { get;private set; }

        /// <summary>
        /// Конструктор с инициализацией словаря страниц.
        /// Constructor with pages dictionary initialization.
        /// </summary>
        [Inject]
        void Construct()
        {
            Pages = new Dictionary<Type, IPageBase>();
        }

        /// <summary>
        /// Добавляет страницу в словарь для дальнейшего управления.
        /// Adds page to dictionary for further management.
        /// </summary>
        /// <param name="key">Ключ типа страницы / Page type key</param>
        /// <param name="page">Объект страницы / Page object</param>
        public void AddPage(Type key, IPageBase page)
        {
            Pages.Add(key,page);
        }

        /// <summary>
        /// Показывает страницу поверх всех остальных.
        /// Shows page on top of all others.
        /// </summary>
        /// <param name="key">Ключ типа страницы / Page type key</param>
        public void ShowOnTop(Type key)
        {
            if (Pages.ContainsKey(key))
            {
                Pages[key].ShowAsLastSibling();
            }
            else
            {
                // Page not found in dictionary
            }
        }

        /// <summary>
        /// Показывает страницу.
        /// Shows page.
        /// </summary>
        /// <param name="key">Ключ типа страницы / Page type key</param>
        public void ShowOn(Type key)
        {

            if (Pages.ContainsKey(key))
            {
                Pages[key].Show();
            }
            else
            {
                // Page not found in dictionary
            }
        }
    }
}
