using System;
using System.Collections.Generic;

namespace Game.Services
{
    /// <summary>
    /// Структура для хранения слота сохранения с метаданными.
    /// Structure for storing save slot with metadata.
    /// </summary>
    [Serializable]
    public class SaveSlot
    {
        public Dictionary<string, string> data = new Dictionary<string, string>();
        public DateTime lastSaveTime;
        public bool isActive;
        
        // Метаданные слота
        public string slotName;
        public int playerLevel;
        public float playtimeHours;
        
        public SaveSlot()
        {
            data = new Dictionary<string, string>();
            lastSaveTime = DateTime.Now;
            isActive = false;
            slotName = "";
            playerLevel = 1;
            playtimeHours = 0f;
        }
    }

    /// <summary>
    /// Интерфейс сервиса сохранения данных с поддержкой слотов.
    /// Data saving and loading service interface with slot support.
    /// </summary>
    public interface ISaveService
    {
        // Работа с активным слотом (обратная совместимость)
        string Load(string key);
        void Save(string key, string value);
        T LoadJson<T>(string key);
        void SaveJson(string key, object data);
        
        // Работа с конкретными слотами
        string Load(string key, int slotId);
        void Save(string key, string value, int slotId);
        T LoadJson<T>(string key, int slotId);
        void SaveJson(string key, object data, int slotId);
        
        // Управление слотами
        void CreateSlot(int slotId, string slotName = "");
        void DeleteSlot(int slotId);
        void CopySlot(int fromSlot, int toSlot);
        SaveSlot GetSlot(int slotId);
        List<SaveSlot> GetAllSlots();
        
        // Работа с активным слотом
        void SetActiveSlot(int slotId);
        int GetActiveSlot();
        SaveSlot GetActiveSlotData();
        
        // Сохранение/загрузка
        void SaveAllSlots();
        void LoadAllSlots();
        void SaveSlot(int slotId);
        void LoadSlot(int slotId);
        
        // Метаданные
        DateTime GetSlotLastSaveTime(int slotId);
        bool IsSlotEmpty(int slotId);
        string GetSlotDisplayName(int slotId);
        bool HasKey(string key);
        bool HasKey(string key, int slotId);
    }
}