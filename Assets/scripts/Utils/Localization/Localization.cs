using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using System;
using System.IO;
using System.Text;
using UnityEditor;

public class Localization : SceneSingleton<Localization>
{
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void initializeOnLoad()
    {
        LocalizationEditor.OnLocalizationImportedEvent -= cleanI2LanguagesSheet;
        LocalizationEditor.OnLocalizationImportedEvent += cleanI2LanguagesSheet;
    }
#endif

    public const string ENGLISH_CODE = "en";
    public const string CHINESE_CODE = "zh-CN";
    [Serializable]
    public class LanguageNames
    {
        public string m_sLanguageCode;
        public string m_sCategory;
        public string m_sKey;
    }
    public List<LanguageNames> m_aoLanguages;

    public static string getCurrentLanguage()
    {
        return I2.Loc.LocalizationManager.CurrentLanguage;
    }

    public static void setLanguage(string _sLanguage)
    {
        I2.Loc.LocalizationManager.CurrentLanguage = _sLanguage;
    }

    public static void updateLocalizedStrings()
    {
        I2.Loc.LocalizationManager.LocalizeAll();
    }

    public static List<string> getAllLanguages()
    {
        List<string> asLanguages = new List<string>();

        asLanguages = LocalizationManager.GetAllLanguages(true);
        return asLanguages;
    }

    public static string getLanguageName(string _sLanguage)
    {
        LanguageNames oLang = Instance.m_aoLanguages.Find(oLang => oLang.m_sLanguageCode == _sLanguage);
        if (oLang == null)
        {
            return _sLanguage;
        }
        return Utils.getTranslation(oLang.m_sKey, oLang.m_sCategory);
    }

    #region TOOLS

    #endregion

    #region Export font chars
#if UNITY_EDITOR
    //private static string[] ms_asLangs = new string[] { "arSA", "jaJP", "koKR", "zhTW", "zhCN", "frFR&itIT&deDE&esMX&esES&plPL&ptBR&nlNL&svSE&nbNO&daDK&fiFI&enUS" };
    private static readonly string[] ms_asLangs = { "Chinese(Simplified)", "Japanese" }; //"Chinese(Traditional)", "Korean", "Arabic"
    //private static int[] ms_aiLangsChars = new int[] { 600, 720, 700, 550, 620, 600 };
    private const string ms_sPath = "Assets/art/fonts/fontChars/";

    [MenuItem("UPTools/Export font chars")]
    private static void exportFontChars()
    {
        string sLanguage = LocalizationManager.CurrentLanguage;
        Debug.Log("Backup: " + sLanguage);

        // Clean I2 Languages Sheet first
        cleanI2LanguagesSheet();

        List<string> sFiles = new List<string>();
        for (int iOutter = 0; iOutter < ms_asLangs.Length; ++iOutter)
        {
            string sLangOutter = ms_asLangs[iOutter];
            StringBuilder sBuilder = new StringBuilder();

            // Get all terms
            string[] asLangs = sLangOutter.Split('&');
            for (int iInner = 0; iInner < asLangs.Length; ++iInner)
            {
                LocalizationManager.CurrentLanguage = asLangs[iInner];
                sBuilder.Append(getTerms());
            }

            string sTerms = sBuilder.ToString();
            IEnumerable<char> aoChars = sTerms.Distinct();

            sBuilder.Clear();
            foreach (char c in aoChars)
            {
                sBuilder.Append(c);
            }

            string sPath = ms_sPath + sLangOutter + "_unique.txt";
            sFiles.Add(sPath);
            writeFile(sPath, sBuilder.ToString());

            Debug.Log("Exported: " + sLangOutter);
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        LocalizationManager.CurrentLanguage = sLanguage;
    }

    public static void cleanI2LanguagesSheet()
    {
        var languageSources = I2.Loc.LocalizationManager.Sources;

        if (languageSources == null || languageSources.Count == 0)
        {
            Debug.LogError("No Language Sources found!");
            return;
        }

        int totalCharsCleaned = 0;

        foreach (var languageSource in languageSources)
        {
            Debug.Log($"Processing source: {languageSource.ownerObject.name}");

            if (languageSource.mTerms == null || languageSource.mTerms.Count == 0)
            {
                Debug.LogWarning("No terms found in source!");
                continue;
            }

            foreach (var term in languageSource.mTerms)
            {
                for (int i = 0; i < term.Languages.Length; ++i)
                {
                    if (string.IsNullOrEmpty(term.Languages[i]))
                        continue;

                    string originalText = term.GetTranslation(i, null, true);
                    string cleanedText = cleanContent(originalText);

                    if (originalText != cleanedText)
                    {
                        term.SetTranslation(i, cleanedText);
                        totalCharsCleaned += originalText.Length - cleanedText.Length;
                        Debug.Log($"Cleaned: {term.Term} [Lang {i}]");
                    }
                }
            }

            // Mark source as dirty
            languageSource.Editor_SetDirty();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Success!</color> Cleaned I2 Sheet. Removed {totalCharsCleaned} hidden characters.");
    }

    static string cleanContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        // Remove invisible characters
        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            @"[\u2060\uFEFF\u200B\u200C\u200D\u00AD\u061C\u180E]",
            ""
        );

        // Normalize space-like characters
        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            @"[\u3000\u00A0]",
            " "
        );

        // Collapse multiple spaces
        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            @" +",
            " "
        );

        return content.Trim();
    }

    private static void writeFile(string _sFile, string _sContent)
    {
        StreamWriter oStream = new StreamWriter(_sFile);
        oStream.WriteLine(_sContent);
        oStream.Close();
    }

    static string getTerms()
    {
        string sTerms = "";
        LanguageSourceData oSource = LocalizationManager.Sources[0];
        List<string> aoCategories = oSource.GetCategories();

        for (int iCategoryIndex = 0; iCategoryIndex < aoCategories.Count; ++iCategoryIndex)
        {
            List<string> aoTerms = getCategoryTerms(oSource, aoCategories[iCategoryIndex]);
            for (int iTermIndex = 0; iTermIndex < aoTerms.Count; ++iTermIndex)
            {
                sTerms += Utils.getTranslation(aoTerms[iTermIndex], aoCategories[iCategoryIndex]);
                //sTerms += oSource.GetTranslation(aoTerms[iTermIndex]);
                //sTerms += oSource.GetTranslation(aoTerms[iTermIndex]).ToLower();
                //sTerms += oSource.GetTranslation(aoTerms[iTermIndex]).ToUpper();
            }
        }
        return sTerms;
    }

    static List<string> getCategoryTerms(LanguageSourceData _oSource, string _sCategory)
    {
        int iTrim = _sCategory.Length + 1;
        List<string> asTerms = _oSource.GetTermsList(_sCategory);
        for (int iTermIndex = 0; iTermIndex < asTerms.Count; ++iTermIndex)
        {
            asTerms[iTermIndex] = asTerms[iTermIndex].Substring(iTrim);
        }
        asTerms.Sort();
        return asTerms;
    }

#endif
    #endregion
}
