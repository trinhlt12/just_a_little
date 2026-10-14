using System;
using System.IO;
using UnityEngine;

namespace OneLittleStep
{
    /// <summary>
    /// JSON save in a single folder (Application.persistentDataPath in the game). Writes go to a temp file first,
    /// the previous good save is kept as .bak, and an unreadable file is copied aside instead of being lost.
    /// </summary>
    public sealed class SaveStore
    {
        public const string FileName = "onelittlestep_save.json";

        readonly string folder;
        public string MainPath => Path.Combine(folder, FileName);
        string TempPath => MainPath + ".tmp";
        string BackupPath => MainPath + ".bak";

        /// <summary>Set when the last Load had to recover from a damaged file.</summary>
        public string LastRecoveryNote { get; private set; }

        public SaveStore(string folder)
        {
            this.folder = folder;
        }

        public SaveData Load()
        {
            LastRecoveryNote = null;
            foreach (var path in new[] { MainPath, TempPath, BackupPath })
            {
                if (!File.Exists(path)) continue;
                if (TryRead(path, out var data)) return data;
                KeepCorruptCopy(path);
            }
            return new SaveData();
        }

        static bool TryRead(string path, out SaveData data)
        {
            data = null;
            try
            {
                var json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return false;
                data = JsonUtility.FromJson<SaveData>(json);
                return data != null && data.version >= 1;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[OneLittleStep] Could not read save {path}: {e.Message}");
                return false;
            }
        }

        void KeepCorruptCopy(string path)
        {
            try
            {
                var copy = Path.Combine(folder, $"onelittlestep_save.corrupt-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Path.GetExtension(path).TrimStart('.')}.json");
                File.Copy(path, copy, true);
                File.Delete(path);
                LastRecoveryNote = copy;
                Debug.LogWarning($"[OneLittleStep] Damaged save kept as {copy}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[OneLittleStep] Could not keep a copy of the damaged save: {e.Message}");
            }
        }

        public bool Save(SaveData data)
        {
            try
            {
                Directory.CreateDirectory(folder);
                data.version = SaveData.CurrentVersion;
                data.lastSavedUtc = DateTime.UtcNow.ToString("o");
                File.WriteAllText(TempPath, JsonUtility.ToJson(data, true));
                if (File.Exists(MainPath)) File.Copy(MainPath, BackupPath, true);
                if (File.Exists(MainPath)) File.Delete(MainPath);
                File.Move(TempPath, MainPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[OneLittleStep] Save failed: {e.Message}");
                return false;
            }
        }
    }
}
