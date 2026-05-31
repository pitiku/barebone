using I2.Loc;
using System.Globalization;

public static class TMPInvariantExtensions
{
    public static char toLower(this char _s, bool _bInvariant = true)
    {
        if (!isTurkicLanguageActive())
        {
            return _bInvariant ? char.ToLowerInvariant(_s) : char.ToLower(_s);
        }
        else
        {
            return _s;
        }
    }

    public static char toUpper(this char _s, bool _bInvariant = true)
    {
        if (!isTurkicLanguageActive())
        {
            return _bInvariant ? char.ToUpperInvariant(_s) : char.ToUpper(_s);
        }
        else
        {
            return _s;
        }
    }

    public static bool isTurkicLanguageActive()
    {
        string lang = LocalizationManager.CurrentLanguage;

        if (string.IsNullOrEmpty(lang))
            return false;

        lang = lang.ToLowerInvariant();

        return lang.Contains("turkish")
            || lang.Contains("azerbaijani")
            || lang.Contains("tatar")
            || lang.Contains("kazakh")
            || lang.Contains("gagauz")
            || lang.Contains("crimean");
    }

    public static bool isTurkicCulture()
    {
        // CurrentCulture is what string.ToUpper()/ToLower() uses by default.
        // CurrentUICulture is for UI resources; not needed for casing bugs.
        string name = CultureInfo.CurrentCulture.Name; // e.g. "tr-TR", "az-Latn-AZ"
        string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName; // e.g. "tr", "az"

        // Languages known to use dotted/dotless I in Latin alphabets:
        // Turkish (tr), Azerbaijani (az), Crimean Tatar (crh), Gagauz (gag), Kazakh Latin (kk*), Tatar (tt)
        // Note: some of these may not appear on all platforms with Latin script tags.
        return lang == "tr"
            || lang == "az"
            || lang == "crh"
            || lang == "gag"
            || lang == "tt"
            || isKazakhLatinOrLikely(name, lang);
    }

    static bool isKazakhLatinOrLikely(string cultureName, string twoLetter)
    {
        // Kazakh is usually "kk-KZ". Latin-script variants may show script tags like "kk-Latn-KZ".
        if (twoLetter != "kk") return false;
        // If you want to be strict and only trigger on Latin script, check "Latn".
        // If you want safe behavior regardless, just return true for "kk".
        return true; // safest: treat all "kk" as potentially affected
                     // strict option:
                     // return cultureName.Contains("Latn");
    }
}