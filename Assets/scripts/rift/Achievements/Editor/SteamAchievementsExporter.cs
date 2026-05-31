// SteamAchievementsExporter.cs
// Exports Steamworks "Localization Tokens" VDF in the format:
//
// "lang"
// {
//   "english"
//   {
//     "Tokens"
//     {
//       "#achievementTitle_finish_run_1"        "My first time travel"
//       "#achievementDescription_finish_run_1"  "Finish your first run."
//     }
//   }
// }
//
// Upload this file in Steamworks -> Achievement Localization (token upload), NOT as stats/ach schema.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using I2.Loc;
using UnityEditor;
using UnityEngine;

public static class SteamAchievementsExporter
{
    private const string OUTPUT_PATH = "SteamworksExports/steam_achievement_tokens.vdf";

    // I2 terms are expected like:
    //  - "achievements/achievementTitle_finish_run_1"
    //  - "achievements/achievementDescription_finish_run_1"
    private const string TITLE_PREFIX = "achievements/achievementTitle_";
    private const string DESC_PREFIX = "achievements/achievementDescription_";

    // Steam token keys we emit:
    //  - "#achievementTitle_finish_run_1"
    //  - "#achievementDescription_finish_run_1"
    private const string TOKEN_TITLE_PREFIX = "#achievementTitle_";
    private const string TOKEN_DESC_PREFIX = "#achievementDescription_";

    [MenuItem("UPTools/Export Steam Achievement Tokens (VDF)")]
    public static void Export()
    {
        LocalizationManager.UpdateSources();

        if (LocalizationManager.Sources == null || LocalizationManager.Sources.Count == 0)
        {
            Debug.LogError("No I2 Localization sources found (I2Languages.asset missing?).");
            return;
        }

        // By convention: use first source (I2Languages). If you use multiple sources, adapt accordingly.
        LanguageSourceData source = LocalizationManager.Sources[0];
        if (source == null)
        {
            Debug.LogError("I2 Localization source is null.");
            return;
        }

        var sb = new StringBuilder(16 * 1024);

        sb.AppendLine("\"lang\"");
        sb.AppendLine("{");

        // Prebuild a list of achievement IDs by scanning title terms.
        // We only export achievements that have both title+desc terms.
        var achIds = new List<string>(128);

        foreach (TermData term in source.mTerms)
        {
            if (term == null) continue;

            string key = term.Term;
            if (string.IsNullOrEmpty(key)) continue;

            if (!key.StartsWith(TITLE_PREFIX, StringComparison.Ordinal))
                continue;

            string id = key.Substring(TITLE_PREFIX.Length);
            if (string.IsNullOrEmpty(id))
                continue;

            string descKey = DESC_PREFIX + id;
            if (source.GetTermData(descKey) == null)
            {
                Debug.LogWarning($"Missing description term for achievement id '{id}' (expected key '{descKey}'). Skipping.");
                continue;
            }

            // Require at least one title translation to avoid generating empty tokens.
            if (!HasAnyNonEmptyTranslation(term.Languages))
            {
                Debug.LogWarning($"Achievement '{id}' has no title translations. Skipping.");
                continue;
            }

            achIds.Add(id);
        }

        // Emit by Steam language section.
        // Only languages that map to Steam codes are exported.
        int exportedLanguageCount = 0;
        int exportedTokenCount = 0;

        for (int langIndex = 0; langIndex < source.mLanguages.Count; langIndex++)
        {
            string i2LangName = source.mLanguages[langIndex].Name;
            string steamLangCode = ToSteamLangCode(i2LangName);
            if (string.IsNullOrEmpty(steamLangCode))
                continue;

            // Gather tokens for this language.
            // We only include tokens with non-empty values for this language.
            var tokensForLang = new List<(string tokenKey, string tokenValue)>(achIds.Count * 2);

            foreach (string id in achIds)
            {
                TermData titleTerm = source.GetTermData(TITLE_PREFIX + id);
                TermData descTerm = source.GetTermData(DESC_PREFIX + id);

                if (titleTerm == null || descTerm == null)
                    continue;

                string titleValue = SafeGet(titleTerm.Languages, langIndex);
                string descValue = SafeGet(descTerm.Languages, langIndex);

                if (!string.IsNullOrEmpty(titleValue))
                {
                    string tokenKey = TOKEN_TITLE_PREFIX + id;
                    tokensForLang.Add((tokenKey, titleValue));
                }

                if (!string.IsNullOrEmpty(descValue))
                {
                    string tokenKey = TOKEN_DESC_PREFIX + id;
                    tokensForLang.Add((tokenKey, descValue));
                }
            }

            if (tokensForLang.Count == 0)
                continue;

            exportedLanguageCount++;

            sb.AppendLine($"\t\"{steamLangCode}\"");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\t\"Tokens\"");
            sb.AppendLine("\t\t{");

            // Stable ordering helps diffs.
            tokensForLang.Sort(static (a, b) => string.CompareOrdinal(a.tokenKey, b.tokenKey));

            foreach ((string tokenKey, string tokenValue) in tokensForLang)
            {
                sb.AppendLine($"\t\t\t\"{Escape(tokenKey)}\" \"{Escape(tokenValue)}\"");
                exportedTokenCount++;
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
        }

        sb.AppendLine("}");

        Directory.CreateDirectory(Path.GetDirectoryName(OUTPUT_PATH) ?? "Assets");
        File.WriteAllText(OUTPUT_PATH, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        AssetDatabase.Refresh();

        Debug.Log($"Exported Steam achievement localization tokens: {exportedTokenCount} tokens across {exportedLanguageCount} languages -> {OUTPUT_PATH}");
        Debug.Log("Steamworks setup reminder: Set achievement Display Name/Description to these tokens (e.g. #achievementTitle_finish_run_1) and upload this VDF in Achievement Localization, then Publish.");
    }

    private static bool HasAnyNonEmptyTranslation(string[] arr)
    {
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++)
        {
            if (!string.IsNullOrEmpty(arr[i]))
                return true;
        }
        return false;
    }

    private static string SafeGet(string[] arr, int index)
    {
        if (arr == null) return string.Empty;
        if ((uint)index >= (uint)arr.Length) return string.Empty;
        return arr[index] ?? string.Empty;
    }

    private static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    // Map I2 language names to Steamworks API language codes (section keys in the VDF).
    private static string ToSteamLangCode(string i2LangName)
    {
        switch (i2LangName)
        {
            case "Arabic": return "arabic";
            case "Bulgarian": return "bulgarian";
            case "Chinese (Simplified)": return "schinese";
            case "Chinese Simplified": return "schinese";
            case "Chinese (Traditional)": return "tchinese";
            case "Czech": return "czech";
            case "Danish": return "danish";
            case "Dutch": return "dutch";
            case "English": return "english";
            case "Finnish": return "finnish";
            case "French": return "french";
            case "German": return "german";
            case "Greek": return "greek";
            case "Hungarian": return "hungarian";
            case "Italian": return "italian";
            case "Japanese": return "japanese";
            case "Korean": return "koreana";
            case "Norwegian": return "norwegian";
            case "Polish": return "polish";
            case "Portuguese": return "portuguese";
            case "Portuguese (Brazil)": return "brazilian";
            case "Romanian": return "romanian";
            case "Russian": return "russian";
            case "Spanish": return "spanish";
            case "Spanish (Spain)": return "spanish";
            case "Spanish (Latin America)": return "latam";
            case "Swedish": return "swedish";
            case "Thai": return "thai";
            case "Turkish": return "turkish";
            case "Ukrainian": return "ukrainian";
            case "Vietnamese": return "vietnamese";
            default:
                return null;
        }
    }
}