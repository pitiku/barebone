using System.Collections.Generic;
using UnityEngine;
using TMPro;
using I2.Loc;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
#endif

public class FontCheck : MonoBehaviour
{
    public List<TMP_FontAsset> m_aoFonts;
    public List<string> mCharSetTool_Languages = new List<string>() { "Chinese (Simplified)", "Russian", "Japanese" };

    string mCharSet = string.Empty;

#if UNITY_EDITOR
    LanguageSourceData mLanguageSource = null;

    [Button("Check Fonts")]
    private void CheckFont()
    {
        LocalizationManager.UpdateSources();
        mLanguageSource = LocalizationManager.Sources[0];
        UpdateCharSets();

        List<char> sMissing;
        foreach (TMP_FontAsset oFont in m_aoFonts)
        {
            oFont.HasCharacters(mCharSet, out sMissing);
            string sOut = "";
            if (sMissing == null)
            {
                sOut = "Nothing Missing!";
            }
            else
            {
                foreach (char c in sMissing)
                {
                    sOut += c;
                }
            }
            Deb.log(oFont.name + ": " + sOut);
            EditorGUIUtility.systemCopyBuffer = sOut;
        }
    }

    void UpdateCharSets()
    {
        mCharSet = "";
        var sb = new HashSet<char>();
        var LanIndexes = new List<int>();
        for (int i = 0; i < mLanguageSource.mLanguages.Count; ++i)
        {
            if (mCharSetTool_Languages.Contains(mLanguageSource.mLanguages[i].Name))
            {
                LanIndexes.Add(i);
            }
        }

        foreach (var termData in mLanguageSource.mTerms)
        {
            for (int i = 0; i < LanIndexes.Count; ++i)
            {
                int iLanguage = LanIndexes[i];
                bool isRTL = LocalizationManager.IsRTL(mLanguageSource.mLanguages[iLanguage].Code);
                AppendToCharSet(sb, termData.Languages[iLanguage], isRTL);
            }
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToArray().OrderBy(c => c).ToArray());
        mCharSet = System.Text.Encoding.UTF8.GetString(bytes);
    }

    void AppendToCharSet(HashSet<char> sb, string text, bool isRTL)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        text = RemoveTagsPrefix(text, "[i2p_");
        text = RemoveTagsPrefix(text, "[i2s_");

        if (isRTL)
        {
            text = RTLFixer.Fix(text);
        }

        foreach (char c in text)
        {
            sb.Add(char.ToLowerInvariant(c));
            sb.Add(char.ToUpperInvariant(c));
        }
    }

    // Given "[i2p_"  it removes all tags that start with that  (e.g. [i2p_Zero]  [i2p_One], etc)
    string RemoveTagsPrefix(string text, string tagPrefix)
    {
        int idx = 0;
        while (idx < text.Length)
        {
            idx = text.IndexOf(tagPrefix);
            if (idx < 0)
            {
                break;
            }

            int idx2 = text.IndexOf(']', idx);
            if (idx2 < 0)
            {
                break;
            }

            text = text.Remove(idx, idx2 - idx + 1);
        }
        return text;

    }
#endif
}
