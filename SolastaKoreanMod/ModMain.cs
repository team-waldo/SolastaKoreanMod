using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using TMPro;

using UnityModManagerNet;
using HarmonyLib;

using GL = UnityEngine.GUILayout;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;

namespace SolastaKoreanMod
{
    public static class ModMain
    {
        // [Conditional("DEBUG")]
        internal static void Log(string msg) => Logger.Log(msg);

        internal static void Error(Exception ex) => Logger?.Error(ex.ToString());
        internal static void Error(string msg) => Logger?.Error(msg);
        internal static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        internal static UnityModManager.ModEntry modEntry;

        private const string FONT_BUNDLE_FILENAME = "sourcehansans.unity3d";

        private static UnityEngine.AssetBundle fontBundle = null;

        public static void Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                Logger = modEntry.Logger;
                modEntry.OnGUI = OnGUI;

                PatchFont();

                var harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                TranslationManager.Instance.Initialize(modEntry.Path);
                ModMain.modEntry = modEntry;
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (GL.Button($"문자열 추출"))
            {
                TranslationManager.Instance.ExportStrings();
            }

            if (TranslationManager.Instance.TranslationLoaded)
            {
                var koreanCulture = CultureInfo.CreateSpecificCulture("ko-KR");
                var creationDate = TranslationManager.Instance.translationData.CreationDate;
                var dateString = creationDate.ToLocalTime().ToString("G", koreanCulture);
                GL.Label($"번역 최종 업데이트 : {dateString}");
            }
            else
            {
                GL.Label("번역 파일이 로드되지 않음");
            }
        }

        private static void PatchFont()
        {
            try
            {
                if (fontBundle == null)
                {
                    var bundlePath = Path.Combine(UnityModManager.modsPath, "SolastaKoreanMod", FONT_BUNDLE_FILENAME);
                    fontBundle = UnityEngine.AssetBundle.LoadFromFile(bundlePath);
                }

                TMP_FontAsset[] allFonts = UnityEngine.Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

                TMP_FontAsset thinOrig = allFonts.First(x => x.name == "Noto-Light SDF" || x.name == "Noto-Thin SDF");
                TMP_FontAsset thinKorean = fontBundle.LoadAsset<TMP_FontAsset>("SourceHanSansK-Light SDF");
                thinOrig.fallbackFontAssetTable.Add(thinKorean);

                TMP_FontAsset regularOrig = allFonts.First(x => x.name == "Noto-Regular SDF");
                TMP_FontAsset regularKorean = fontBundle.LoadAsset<TMP_FontAsset>("SourceHanSansK-Regular SDF");
                regularOrig.fallbackFontAssetTable.Add(regularKorean);

                TMP_FontAsset boldOrig = allFonts.First(x => x.name == "Noto-Bold SDF");
                TMP_FontAsset boldKorean = fontBundle.LoadAsset<TMP_FontAsset>("SourceHanSansK-Bold SDF");
                boldOrig.fallbackFontAssetTable.Add(boldKorean);

                TMP_FontAsset liberationSans = allFonts.First(x => x.name == "LiberationSans SDF");
                liberationSans.fallbackFontAssetTable.Add(regularKorean);
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }
    }
}
