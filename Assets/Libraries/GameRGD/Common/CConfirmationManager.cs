using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class CConfirmationManager : CMonoSingleton<CConfirmationManager>
    {
        #region static
        public static readonly string USER_CONFIRMATION_FILENAME = "user_confirmation.dug";

        public static bool ExistUserConfirmationFile() => File.Exists($"{Application.persistentDataPath}/{USER_CONFIRMATION_FILENAME}");

        public static void SaveUserConfirmationToFile(List<string> data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText($"{Application.persistentDataPath}/{USER_CONFIRMATION_FILENAME}", json);
        }

        public static bool LoadUserConfirmationFromFile(out List<string> repository)
        {
            var path = $"{Application.persistentDataPath}/{USER_CONFIRMATION_FILENAME}";

            CLogger.Log($"LoadUserConfirmationFromFile: {path}");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                try
                {
                    repository = JsonConvert.DeserializeObject<List<string>>(json);
                }
                catch (Exception e)
                {
                    CLogger.Log($"json 파싱 실패 {e}");
                    repository = default;
                    return false;
                }
                return true;
            }

            repository = default;
            return false;
        }
        #endregion static

        private List<string> m_ConfirmationSet = default;
        private bool m_IsSaving = default;

        public void Load()
        {
            if (LoadUserConfirmationFromFile(out m_ConfirmationSet) == false)
            {
                m_ConfirmationSet = new List<string>();
                SaveUserConfirmationToFile(m_ConfirmationSet);
            }
        }

        public void AddConfirmation(string uid)
        {
            m_ConfirmationSet.Add(uid);
            SaveAsync();
        }

        public void RemoveConfirmation(string uid)
        {
            if (m_ConfirmationSet.Remove(uid)) SaveAsync();
        }

        private async void SaveAsync()
        {
            if (m_IsSaving == false)
            {
                m_IsSaving = true;
                await UniTask.Delay(5000);
                SaveUserConfirmationToFile(m_ConfirmationSet);
                m_IsSaving = false;
            }
        }

        public bool HasConfirmation(string uid) => m_ConfirmationSet.Contains(uid);
    }
}
