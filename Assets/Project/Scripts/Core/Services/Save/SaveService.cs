using UnityEngine;
using Zenject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Services
{
    /// <summary>
    /// Сервис сохранения и загрузки данных с поддержкой множественных слотов. Использует PlayerPrefs для хранения.
    /// Save and load data service with multiple slots support. Uses PlayerPrefs for storage.
    /// </summary>
    public class SaveService : ISaveService
    {
        private const string SLOTS_COUNT_KEY = "SaveSlots_Count";
        private const string SLOT_PREFIX = "SaveSlot_";
        private const string ACTIVE_SLOT_KEY = "ActiveSlot";
        private const int DEFAULT_SLOT = 0;
        private const int MAX_SLOTS = 10;
        
        private Dictionary<int, SaveSlot> saveSlots = new Dictionary<int, SaveSlot>();
        private int activeSlotId = DEFAULT_SLOT;

        /// <summary>
        /// Конструктор сервиса с инициализацией слотов сохранения.
        /// Service constructor with save slots initialization.
        /// </summary>
        [Inject]
        private void Construct()
        {
            // Save service initialized
            
            LoadAllSlots();
            
            // Создаем слот по умолчанию если нет слотов
            if (saveSlots.Count == 0)
            {
                CreateSlot(DEFAULT_SLOT, "Default Save");
            }
            
            // Загружаем активный слот
            var activeSlot = PlayerPrefs.GetInt(ACTIVE_SLOT_KEY, DEFAULT_SLOT);
            if (saveSlots.ContainsKey(activeSlot))
            {
                activeSlotId = activeSlot;
            }
            else
            {
                activeSlotId = DEFAULT_SLOT;
            }
            
            // Save service initialized successfully
        }

        #region Active Slot Methods (Backward Compatibility)
        /// <summary>
        /// Сохраняет значение по ключу в активном слоте.
        /// Saves value by key in active slot.
        /// </summary>
        /// <param name="key">Ключ / Key</param>
        /// <param name="value">Значение / Value</param>
        public void Save(string key, string value)
        {
            Save(key, value, activeSlotId);
        }

        /// <summary>
        /// Загружает значение по ключу из активного слота.
        /// Loads value by key from active slot.
        /// </summary>
        /// <param name="key">Ключ / Key</param>
        /// <returns>Значение или null / Value or null</returns>
        public string Load(string key)
        {
            return Load(key, activeSlotId);
        }

        /// <summary>
        /// Загружает объект из JSON по ключу из активного слота.
        /// Loads object from JSON by key from active slot.
        /// </summary>
        /// <typeparam name="T">Тип объекта / Object type</typeparam>
        /// <param name="key">Ключ / Key</param>
        /// <returns>Объект или default / Object or default</returns>
        public T LoadJson<T>(string key)
        {
            return LoadJson<T>(key, activeSlotId);
        }
       
        /// <summary>
        /// Сохраняет объект в JSON по ключу в активном слоте.
        /// Saves object to JSON by key in active slot.
        /// </summary>
        /// <param name="key">Ключ / Key</param>
        /// <param name="data">Объект для сохранения / Object to save</param>
        public void SaveJson(string key, object data)
        {
            SaveJson(key, data, activeSlotId);
        }
        
        /// <summary>
        /// Проверяет наличие ключа в активном слоте.
        /// Checks if key exists in active slot.
        /// </summary>
        /// <param name="key">Ключ / Key</param>
        /// <returns>True если ключ существует / True if key exists</returns>
        public bool HasKey(string key)
        {
            return HasKey(key, activeSlotId);
        }
        #endregion

        #region Slot-Specific Methods
        /// <summary>
        /// Сохраняет значение по ключу в указанном слоте.
        /// Saves value by key in specified slot.
        /// </summary>
        /// <param name="key">Ключ / Key</param>
        /// <param name="value">Значение / Value</param>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        public void Save(string key, string value, int slotId)
        {
            if (!ValidateSlotId(slotId)) return;
            
            EnsureSlotExists(slotId);
            saveSlots[slotId].data[key] = value;
            saveSlots[slotId].lastSaveTime = DateTime.Now;
            
            SaveSlot(slotId);
            // Value saved to slot
        }

        /// <summary>
        /// Загружает значение по ключу из указанного слота.
        /// Loads value by key from specified slot.
        /// </summary>
        /// <param name="key">Ключ / Key</param>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        /// <returns>Значение или null / Value or null</returns>
        public string Load(string key, int slotId)
        {
            if (!ValidateSlotId(slotId)) return null;
            if (!saveSlots.ContainsKey(slotId)) return null;
            
            var result = saveSlots[slotId].data.TryGetValue(key, out var value) ? value : null;
            // Value loaded or not found
            return result;
        }

        /// <summary>
        /// Загружает объект из JSON по ключу из указанного слота.
        /// Loads object from JSON by key from specified slot.
        /// </summary>
        /// <typeparam name="T">Тип объекта / Object type</typeparam>
        /// <param name="key">Ключ / Key</param>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        /// <returns>Объект или default / Object or default</returns>
        public T LoadJson<T>(string key, int slotId)
        {
            var jsonString = Load(key, slotId);
            if (string.IsNullOrEmpty(jsonString)) return default(T);
            
            try
            {
                var result = JsonConvert.DeserializeObject<T>(jsonString);
                // JSON object loaded successfully
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] ✗ Failed to deserialize JSON '{key}' from slot {slotId}: {e.Message}");
                return default(T);
            }
        }
       
        /// <summary>
        /// Сохраняет объект в JSON по ключу в указанном слоте.
        /// Saves object to JSON by key in specified slot.
        /// </summary>
        /// <param name="key">Ключ / Key</param>
        /// <param name="data">Объект для сохранения / Object to save</param>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        public void SaveJson(string key, object data, int slotId)
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(data);
                Save(key, jsonString, slotId);
                // JSON object saved successfully
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] ✗ Failed to serialize JSON '{key}' for slot {slotId}: {e.Message}");
            }
        }
        
        /// <summary>
        /// Проверяет наличие ключа в указанном слоте.
        /// Checks if key exists in specified slot.
        /// </summary>
        /// <param name="key">Ключ / Key</param>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        /// <returns>True если ключ существует / True if key exists</returns>
        public bool HasKey(string key, int slotId)
        {
            if (!ValidateSlotId(slotId)) return false;
            if (!saveSlots.ContainsKey(slotId)) return false;
            
            return saveSlots[slotId].data.ContainsKey(key);
        }
        #endregion

        #region Slot Management
        /// <summary>
        /// Создает новый слот сохранения.
        /// Creates new save slot.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        /// <param name="slotName">Имя слота / Slot name</param>
        public void CreateSlot(int slotId, string slotName = "")
        {
            if (!ValidateSlotId(slotId)) return;
            
            if (saveSlots.ContainsKey(slotId))
            {
            // Slot already exists
                return;
            }
            
            var newSlot = new SaveSlot
            {
                slotName = string.IsNullOrEmpty(slotName) ? $"Save Slot {slotId}" : slotName,
                lastSaveTime = DateTime.Now
            };
            
            saveSlots[slotId] = newSlot;
            SaveSlot(slotId);
            
            // Slot created successfully
        }

        /// <summary>
        /// Удаляет слот сохранения.
        /// Deletes save slot.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        public void DeleteSlot(int slotId)
        {
            if (!ValidateSlotId(slotId)) return;
            if (slotId == DEFAULT_SLOT)
            {
            // Cannot delete default slot
                return;
            }
            
            if (saveSlots.ContainsKey(slotId))
            {
                saveSlots.Remove(slotId);
                PlayerPrefs.DeleteKey(SLOT_PREFIX + slotId);
                
                // Переключаемся на слот по умолчанию если удаляем активный
                if (activeSlotId == slotId)
                {
                    SetActiveSlot(DEFAULT_SLOT);
                }
                
                // Slot deleted successfully
            }
        }

        /// <summary>
        /// Копирует слот сохранения.
        /// Copies save slot.
        /// </summary>
        /// <param name="fromSlot">Исходный слот / Source slot</param>
        /// <param name="toSlot">Целевой слот / Target slot</param>
        public void CopySlot(int fromSlot, int toSlot)
        {
            if (!ValidateSlotId(fromSlot) || !ValidateSlotId(toSlot)) return;
            if (!saveSlots.ContainsKey(fromSlot))
            {
            // Source slot doesn't exist
                return;
            }
            
            var sourceSlot = saveSlots[fromSlot];
            var newSlot = new SaveSlot
            {
                data = new Dictionary<string, string>(sourceSlot.data),
                slotName = sourceSlot.slotName + " (Copy)",
                playerLevel = sourceSlot.playerLevel,
                playtimeHours = sourceSlot.playtimeHours,
                lastSaveTime = DateTime.Now
            };
            
            saveSlots[toSlot] = newSlot;
            SaveSlot(toSlot);
            
            // Slot copied successfully
        }

        /// <summary>
        /// Получает данные слота.
        /// Gets slot data.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        /// <returns>Данные слота / Slot data</returns>
        public SaveSlot GetSlot(int slotId)
        {
            return saveSlots.TryGetValue(slotId, out var slot) ? slot : null;
        }

        /// <summary>
        /// Получает все слоты сохранения.
        /// Gets all save slots.
        /// </summary>
        /// <returns>Список всех слотов / List of all slots</returns>
        public List<SaveSlot> GetAllSlots()
        {
            return saveSlots.Values.ToList();
        }
        #endregion

        #region Active Slot Management
        /// <summary>
        /// Устанавливает активный слот.
        /// Sets active slot.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        public void SetActiveSlot(int slotId)
        {
            if (!ValidateSlotId(slotId)) return;
            
            EnsureSlotExists(slotId);
            activeSlotId = slotId;
            PlayerPrefs.SetInt(ACTIVE_SLOT_KEY, activeSlotId);
            PlayerPrefs.Save();
            
            // Active slot changed
        }

        /// <summary>
        /// Получает идентификатор активного слота.
        /// Gets active slot identifier.
        /// </summary>
        /// <returns>Идентификатор активного слота / Active slot identifier</returns>
        public int GetActiveSlot()
        {
            return activeSlotId;
        }
        
        /// <summary>
        /// Получает данные активного слота.
        /// Gets active slot data.
        /// </summary>
        /// <returns>Данные активного слота / Active slot data</returns>
        SaveSlot ISaveService.GetActiveSlotData()
        {
            return GetSlot(activeSlotId);
        }
        #endregion

        #region Save/Load Operations
        /// <summary>
        /// Сохраняет все слоты.
        /// Saves all slots.
        /// </summary>
        public void SaveAllSlots()
        {
            foreach (var kvp in saveSlots)
            {
                SaveSlot(kvp.Key);
            }
            PlayerPrefs.SetInt(SLOTS_COUNT_KEY, saveSlots.Count);
            PlayerPrefs.Save();
            
            // All slots saved
        }

        /// <summary>
        /// Загружает все слоты.
        /// Loads all slots.
        /// </summary>
        public void LoadAllSlots()
        {
            saveSlots.Clear();
            
            var slotsCount = PlayerPrefs.GetInt(SLOTS_COUNT_KEY, 0);
            
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                var slotKey = SLOT_PREFIX + i;
                if (PlayerPrefs.HasKey(slotKey))
                {
                    LoadSlot(i);
                }
            }
            
            // All slots loaded
        }

        /// <summary>
        /// Сохраняет конкретный слот.
        /// Saves specific slot.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        public void SaveSlot(int slotId)
        {
            if (!saveSlots.ContainsKey(slotId)) return;
            
            try
            {
                var jsonString = JsonConvert.SerializeObject(saveSlots[slotId]);
                PlayerPrefs.SetString(SLOT_PREFIX + slotId, jsonString);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to save slot {slotId}: {e.Message}");
            }
        }

        /// <summary>
        /// Загружает конкретный слот.
        /// Loads specific slot.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        public void LoadSlot(int slotId)
        {
            var slotKey = SLOT_PREFIX + slotId;
            if (!PlayerPrefs.HasKey(slotKey)) return;
            
            try
            {
                var jsonString = PlayerPrefs.GetString(slotKey);
                var slot = JsonConvert.DeserializeObject<SaveSlot>(jsonString);
                if (slot != null)
                {
                    saveSlots[slotId] = slot;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to load slot {slotId}: {e.Message}");
            }
        }
        #endregion

        #region Metadata
        /// <summary>
        /// Получает время последнего сохранения слота.
        /// Gets slot last save time.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        /// <returns>Время последнего сохранения / Last save time</returns>
        public DateTime GetSlotLastSaveTime(int slotId)
        {
            return saveSlots.TryGetValue(slotId, out var slot) ? slot.lastSaveTime : DateTime.MinValue;
        }

        /// <summary>
        /// Проверяет, пуст ли слот.
        /// Checks if slot is empty.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        /// <returns>True если слот пуст / True if slot is empty</returns>
        public bool IsSlotEmpty(int slotId)
        {
            return !saveSlots.ContainsKey(slotId) || saveSlots[slotId].data.Count == 0;
        }

        /// <summary>
        /// Получает отображаемое имя слота.
        /// Gets slot display name.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        /// <returns>Отображаемое имя / Display name</returns>
        public string GetSlotDisplayName(int slotId)
        {
            return saveSlots.TryGetValue(slotId, out var slot) ? slot.slotName : $"Empty Slot {slotId}";
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// Проверяет корректность идентификатора слота.
        /// Validates slot identifier.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        /// <returns>True если ID корректен / True if ID is valid</returns>
        private bool ValidateSlotId(int slotId)
        {
            if (slotId < 0 || slotId >= MAX_SLOTS)
            {
                Debug.LogError($"[SaveService] Invalid slot ID: {slotId}. Must be between 0 and {MAX_SLOTS - 1}");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Обеспечивает существование слота, создавая его при необходимости.
        /// Ensures slot exists, creating it if necessary.
        /// </summary>
        /// <param name="slotId">Идентификатор слота / Slot identifier</param>
        private void EnsureSlotExists(int slotId)
        {
            if (!saveSlots.ContainsKey(slotId))
            {
                CreateSlot(slotId);
            }
        }
        #endregion
    }
}