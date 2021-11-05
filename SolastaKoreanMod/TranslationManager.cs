using HarmonyLib;
using I2.Loc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SolastaKoreanMod.ModMain;

namespace SolastaKoreanMod
{
    public class TranslationManager
    {
        private const string TRANSLATION_FILENAME = "translation.json";

        public const string LANGUAGE_CODE = "ko";
        public const string LANGUAGE_NAME = "한국어";

        private string TranslationFilePath() => Path.Combine(ModDirectory, TRANSLATION_FILENAME);

        public bool Initialized { get; private set; } = false;
        public bool TranslationLoaded { get; private set; } = false;
        private string ModDirectory { get; set; }

        private readonly GithubRepoClient githubClient;
        private readonly JsonSerializer serializer;

        internal TranslationData translationData;

        // singleton
        public static TranslationManager Instance
        {
            get => _instance ?? (_instance = new TranslationManager());
        }

        private static TranslationManager _instance;

        private TranslationManager()
        {
            githubClient = new GithubRepoClient();

            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };
            serializer = JsonSerializer.Create(settings);
        }

        public void Initialize(string modDirectory)
        {
            if (Initialized)
                return;

            ModDirectory = modDirectory;
            translationData = LoadTranslation();

            if (translationData is null)
            {
                Log("Failed to load translation data. Disabling the mod...");
                TranslationLoaded = false;
                Initialized = true;
                return;
            }

            var languageSourceData = LocalizationManager.Sources[0];
            languageSourceData.AddLanguage("한국어", "ko");

            int languageIndex = languageSourceData.GetLanguageIndex("한국어");
            int translated = 0;
            int removed = 0;

            foreach (var item in translationData.Strings)
            {
                var term = languageSourceData.GetTermData(item.Key);
                if (term != null)
                {
                    term.Languages[languageIndex] = item.Value;
                    translated++;
                }
                else
                {
                    removed++;
                }
            }

            int total = languageSourceData.mTerms.Count;
            ModMain.Log($"Translated ({translated}/{total})");
            ModMain.Log($"{removed} terms removed");

            Log("Added Korean language.");
            TranslationLoaded = true;
            Initialized = true;
        }

        private TranslationData LoadTranslation()
        {
            var cache = LoadCachedTranslation();

            Log("Checking latest translation release...");
            var release = githubClient.GetLatestRelease();
            if (release is null || release.assets.Count == 0)
            {
                Log("Failed to load latest release data");
                return cache;
            }

            if (!(cache is null))
                Log($"Cache file release : {cache.CreationDate.ToLocalTime()}");

            Log($"Latest release: {release.published_at.ToLocalTime()}");
            if (!(cache is null) && release.published_at <= (cache.CreationDate.AddHours(1)))
            {
                Log("Already have latest version.");
                return cache;
            }

            Log("Downloading latest translation...");
            var newData = githubClient.DoanloadAsset(release.assets[0]);
            if (!(newData is null))
            {
                SaveTranslationCache(newData);
                Log("Downloaded and saved translation data.");
            }

            return newData ?? cache;
        }

        private TranslationData LoadCachedTranslation()
        {
            string filePath = TranslationFilePath();
            if (!File.Exists(filePath))
            {
                Log("Translation cache file not found.");
                return null;
            }

            using (StreamReader sr = new StreamReader(filePath))
            using (JsonTextReader jtr = new JsonTextReader(sr))
            {
                var td = serializer.Deserialize<TranslationData>(jtr);
                Log("Translation file cache loaded.");
                return td;
            }
        }

        private void SaveTranslationCache(TranslationData td)
        {
            using (StreamWriter sw = new StreamWriter(TranslationFilePath()))
            using (JsonTextWriter jtw = new JsonTextWriter(sw))
            {
                serializer.Serialize(jtw, td);
            }
        }

        public void ExportStrings()
        {
            var sourceData = LocalizationManager.Sources[0];
            int englishIndex = sourceData.GetLanguageIndexFromCode("en");

            var termDict = sourceData.mTerms.ToDictionary(x => x.Term, x => x.Languages[englishIndex]);

            string outputPath = Path.Combine(modEntry.Path, "en_export.json");
            using (StreamWriter sw = new StreamWriter(outputPath))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                serializer.Serialize(jw, termDict);
            }
        }

        [HarmonyPatch(typeof(GameManager), "BindServiceSettings")]
        internal static class GameManager_BindServiceSettings_Patch
        {
            public static void Prefix(Dictionary<string, string> ___languageByCode)
            {
                if (___languageByCode != null && Instance.TranslationLoaded)
                    if (!___languageByCode.ContainsKey(LANGUAGE_CODE))
                        ___languageByCode.Add(LANGUAGE_CODE, LANGUAGE_NAME);
            }
        }

        [HarmonyPatch(typeof(SettingDropListItem), "Bind")]
        internal static class SettingDropListItem_Bind_Patch
        {
            public static void Postfix(
                SettingTypeDropListAttribute ___settingTypeDropListAttribute,
                GuiDropdown ___dropList)
            {
                if (___settingTypeDropListAttribute?.Name != "TextLanguage") return;
                if (!Instance.TranslationLoaded) return;

                int top = ___settingTypeDropListAttribute.Items.Count();
                string[] items = new string[top + 1];
                Array.Copy(___settingTypeDropListAttribute.Items, items, top);
                items[top] = LANGUAGE_CODE;

                ___settingTypeDropListAttribute.Items = items;
                ___dropList.options.Add(new GuiDropdown.OptionDataAdvanced
                {
                    text = LANGUAGE_NAME,
                    TooltipContent = LANGUAGE_NAME,
                });

            }
        }
    }
}
